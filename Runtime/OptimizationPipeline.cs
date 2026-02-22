using System.Collections.Generic;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// 全モジュールを順序通り呼び出すメイン最適化フロー。
    /// </summary>
    public static class OptimizationPipeline
    {
        /// <summary>
        /// メインの最適化フローを実行する。
        /// </summary>
        public static void Execute(GameObject avatarRoot, TextureCropSettings settings)
        {
            TCOLogger.Info("Pipeline", "最適化を開始します", avatarRoot.name);

            // 1. Avatar配下の全RendererからMesh・Materialを収集
            var entries = RendererCollector.Collect(avatarRoot);
            if (entries.Count == 0)
            {
                TCOLogger.Info("Pipeline", "Rendererが見つかりませんでした。スキップします", avatarRoot.name);
                return;
            }

            // 除外マテリアルセットを構築
            var excludedMaterials = new HashSet<Material>();
            if (settings.Entries != null)
            {
                foreach (var entry in settings.Entries)
                {
                    if (entry.Excluded && entry.Material != null)
                        excludedMaterials.Add(entry.Material);
                }
            }

            // 2. テクスチャ単位でグループ化
            var textureGroups = TextureGroupBuilder.Build(entries, excludedMaterials);
            if (textureGroups.Count == 0)
            {
                TCOLogger.Info("Pipeline", "最適化対象のテクスチャが見つかりませんでした", avatarRoot.name);
                return;
            }

            TCOLogger.Info("Pipeline", $"{textureGroups.Count}個のテクスチャグループを検出しました", avatarRoot.name);

            // === Phase 1: 解析 — 各テクスチャグループのUsedRectと最適化サイズを算出 ===
            var analysisResults = new Dictionary<Texture2D, AnalysisResult>();

            foreach (var kvp in textureGroups)
            {
                var texture = kvp.Key;
                var group = kvp.Value;

                var result = AnalyzeTextureGroup(texture, group);
                if (result != null)
                    analysisResults[texture] = result;
            }

            if (analysisResults.Count == 0)
            {
                TCOLogger.Info("Pipeline", "最適化可能なテクスチャがありませんでした", avatarRoot.name);
                return;
            }

            // === Phase 2: 適用 — テクスチャ再構成・メッシュリマップ・マテリアル差し替え ===
            // マテリアルとメッシュのマップをグループ横断で共有
            var materialMap = new Dictionary<Material, Material>();
            var meshMap = new Dictionary<Mesh, Mesh>();

            foreach (var kvp in analysisResults)
            {
                var texture = kvp.Key;
                var analysis = kvp.Value;
                var group = textureGroups[texture];

                ApplyOptimization(texture, group, analysis, materialMap, meshMap);
            }

            // Rendererに新しいメッシュ・マテリアルを適用
            ApplyToRenderers(entries, materialMap, meshMap);

            TCOLogger.Info("Pipeline", "最適化が完了しました", avatarRoot.name);
        }

        private class AnalysisResult
        {
            public Rect UsedRect;
            public int OriginalSize;
            public int OptimizedSize;
        }

        private static AnalysisResult AnalyzeTextureGroup(Texture2D texture, TextureGroup group)
        {
            var allIslandBounds = new List<Rect>();
            var processedMeshes = new HashSet<Mesh>();

            foreach (var reference in group.References)
            {
                if (processedMeshes.Contains(reference.Mesh))
                    continue;
                processedMeshes.Add(reference.Mesh);

                var islandBounds = UVIslandDetector.DetectIslandBounds(reference.Mesh);
                if (islandBounds == null)
                {
                    TCOLogger.Warning("Pipeline",
                        "UV0が範囲外またはUVなし。スキップします",
                        texture.name);
                    return null;
                }

                allIslandBounds.AddRange(islandBounds);
            }

            if (allIslandBounds.Count == 0)
            {
                TCOLogger.Info("Pipeline", "UV島が見つかりませんでした。スキップします", texture.name);
                return null;
            }

            var usedRect = UVRectCalculator.CalculateUsedRect(allIslandBounds);
            int originalSize = Mathf.Max(texture.width, texture.height);
            int optimizedSize = PowerOfTwoCalculator.Calculate(usedRect, originalSize);

            if (!PowerOfTwoCalculator.IsWorthOptimizing(originalSize, optimizedSize))
            {
                TCOLogger.Info("Pipeline",
                    $"サイズ削減が不十分です（{originalSize} → {optimizedSize}）。スキップします",
                    texture.name);
                return null;
            }

            TCOLogger.Info("Pipeline",
                $"テクスチャを最適化します: {originalSize} → {optimizedSize}",
                texture.name);

            return new AnalysisResult
            {
                UsedRect = usedRect,
                OriginalSize = originalSize,
                OptimizedSize = optimizedSize
            };
        }

        private static void ApplyOptimization(
            Texture2D texture,
            TextureGroup group,
            AnalysisResult analysis,
            Dictionary<Material, Material> materialMap,
            Dictionary<Mesh, Mesh> meshMap)
        {
            // テクスチャを複製・再構成
            Texture2D newTexture;
            using (new TextureReadableHandler(texture))
            {
                newTexture = TextureRebuilder.Rebuild(texture, analysis.UsedRect, analysis.OptimizedSize);
            }

            // メッシュを複製・UV0リマップ（未処理のメッシュのみ）
            foreach (var reference in group.References)
            {
                if (!meshMap.ContainsKey(reference.Mesh))
                {
                    meshMap[reference.Mesh] = MeshRemapper.Remap(reference.Mesh, analysis.UsedRect);
                }
            }

            // マテリアルを複製・テクスチャプロパティを差し替え
            foreach (var reference in group.References)
            {
                if (!materialMap.ContainsKey(reference.Material))
                {
                    // 新規コピーを作成
                    var textureReplacement = new Dictionary<string, Texture2D>
                    {
                        { reference.PropertyName, newTexture }
                    };
                    materialMap[reference.Material] = MaterialRebuilder.Rebuild(
                        reference.Material, textureReplacement);
                }
                else
                {
                    // 既存コピーのテクスチャプロパティを追加差し替え
                    materialMap[reference.Material].SetTexture(reference.PropertyName, newTexture);
                }
            }
        }

        private static void ApplyToRenderers(
            List<RendererEntry> entries,
            Dictionary<Material, Material> materialMap,
            Dictionary<Mesh, Mesh> meshMap)
        {
            foreach (var entry in entries)
            {
                bool modified = false;

                // マテリアルの差し替え
                var materials = entry.Renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null && materialMap.TryGetValue(materials[i], out var newMat))
                    {
                        materials[i] = newMat;
                        modified = true;
                    }
                }
                if (modified)
                    entry.Renderer.sharedMaterials = materials;

                // メッシュの差し替え
                if (meshMap.TryGetValue(entry.Mesh, out var newMesh))
                {
                    if (entry.Renderer is SkinnedMeshRenderer smr)
                        smr.sharedMesh = newMesh;
                    else if (entry.Renderer is MeshRenderer)
                    {
                        var meshFilter = entry.Renderer.GetComponent<MeshFilter>();
                        if (meshFilter != null)
                            meshFilter.sharedMesh = newMesh;
                    }
                }
            }
        }
    }
}
