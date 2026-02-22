using System.Collections.Generic;
using System.Linq;
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

            // 3. 各テクスチャグループを処理
            foreach (var kvp in textureGroups)
            {
                var texture = kvp.Key;
                var group = kvp.Value;

                ProcessTextureGroup(texture, group, entries);
            }

            TCOLogger.Info("Pipeline", "最適化が完了しました", avatarRoot.name);
        }

        private static void ProcessTextureGroup(Texture2D texture, TextureGroup group, List<RendererEntry> entries)
        {
            // 4. UV島を検出 → 各島のAABBを算出 → 和集合でUsedRect確定
            var allIslandBounds = new List<Rect>();

            // 同一テクスチャを参照する全メッシュのUV0を収集
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
                    return;
                }

                allIslandBounds.AddRange(islandBounds);
            }

            if (allIslandBounds.Count == 0)
            {
                TCOLogger.Info("Pipeline", "UV島が見つかりませんでした。スキップします", texture.name);
                return;
            }

            var usedRect = UVRectCalculator.CalculateUsedRect(allIslandBounds);

            // 5. UsedRectから必要最小の2のべき乗サイズを算出
            int originalSize = Mathf.Max(texture.width, texture.height);
            int optimizedSize = PowerOfTwoCalculator.Calculate(usedRect, originalSize);

            if (!PowerOfTwoCalculator.IsWorthOptimizing(originalSize, optimizedSize))
            {
                TCOLogger.Info("Pipeline",
                    $"サイズ削減が不十分です（{originalSize} → {optimizedSize}）。スキップします",
                    texture.name);
                return;
            }

            TCOLogger.Info("Pipeline",
                $"テクスチャを最適化します: {originalSize} → {optimizedSize}",
                texture.name);

            // 6-7. テクスチャを複製・再構成
            Texture2D newTexture;
            using (new TextureReadableHandler(texture))
            {
                newTexture = TextureRebuilder.Rebuild(texture, usedRect, optimizedSize);
            }

            // 8. メッシュを複製・UV0リマップ
            var meshMap = new Dictionary<Mesh, Mesh>();
            foreach (var reference in group.References)
            {
                if (!meshMap.ContainsKey(reference.Mesh))
                {
                    meshMap[reference.Mesh] = MeshRemapper.Remap(reference.Mesh, usedRect);
                }
            }

            // 9. マテリアルを複製・テクスチャプロパティを差し替え
            var materialMap = new Dictionary<Material, Material>();
            foreach (var reference in group.References)
            {
                if (!materialMap.ContainsKey(reference.Material))
                {
                    var textureReplacement = new Dictionary<string, Texture2D>
                    {
                        { reference.PropertyName, newTexture }
                    };
                    materialMap[reference.Material] = MaterialRebuilder.Rebuild(
                        reference.Material, textureReplacement);
                }
                else
                {
                    // 同一マテリアルの別プロパティにもテクスチャを設定
                    materialMap[reference.Material].SetTexture(reference.PropertyName, newTexture);
                }
            }

            // Rendererに新しいメッシュ・マテリアルを適用
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
