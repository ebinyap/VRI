using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TextureCropOptimizer.Editor
{
    /// <summary>
    /// TextureCropSettingsのカスタムインスペクター。
    /// テクスチャ検知ボタンとマテリアルエントリリストを表示する。
    /// </summary>
    [CustomEditor(typeof(TextureCropSettings))]
    public class TextureCropSettingsEditor : UnityEditor.Editor
    {
        // 検知結果のキャッシュ（テクスチャごとのサイズ情報）
        private Dictionary<Texture2D, (int Original, int Optimized)> _sizeCache;

        public override void OnInspectorGUI()
        {
            var settings = (TextureCropSettings)target;

            EditorGUILayout.Space();

            // テクスチャ検知ボタン
            if (GUILayout.Button("テクスチャを検知", GUILayout.Height(30)))
            {
                DetectTextures(settings);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("マテリアルエントリ", EditorStyles.boldLabel);

            if (settings.Entries == null || settings.Entries.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "マテリアルエントリがありません。「テクスチャを検知」ボタンを押してください。",
                    MessageType.Info);
                return;
            }

            // サマリー表示
            if (_sizeCache != null && _sizeCache.Count > 0)
            {
                DrawSummary(settings);
                EditorGUILayout.Space();
            }

            // エントリリスト表示
            for (int i = 0; i < settings.Entries.Count; i++)
            {
                var entry = settings.Entries[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // マテリアルフィールド
                EditorGUI.BeginChangeCheck();
                entry.Material = (Material)EditorGUILayout.ObjectField(
                    "Material", entry.Material, typeof(Material), false);

                // テクスチャサイズ情報（読み取り専用）
                if (entry.Material != null && !entry.Excluded)
                {
                    DrawTextureSizeInfo(entry.Material);
                }

                // 除外チェックボックス
                entry.Excluded = EditorGUILayout.Toggle("除外する", entry.Excluded);

                // UV回転ドロップダウン（除外時はグレーアウト）
                EditorGUI.BeginDisabledGroup(entry.Excluded);
                entry.UVRotation = (UVRotationMode)EditorGUILayout.EnumPopup(
                    "UV回転", entry.UVRotation);
                EditorGUI.EndDisabledGroup();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(settings, "TextureCropSettings変更");
                    EditorUtility.SetDirty(settings);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
        }

        private void DrawSummary(TextureCropSettings settings)
        {
            if (_sizeCache == null)
                return;

            int textureCount = 0;
            long totalOriginalBytes = 0;
            long totalOptimizedBytes = 0;

            // 除外されていないマテリアルに関連するテクスチャのみ集計
            var excludedMats = new HashSet<Material>();
            foreach (var entry in settings.Entries)
            {
                if (entry.Excluded && entry.Material != null)
                    excludedMats.Add(entry.Material);
            }

            foreach (var kvp in _sizeCache)
            {
                var original = kvp.Value.Original;
                var optimized = kvp.Value.Optimized;
                // ピクセル数ベースの概算VRAM（RGBA 4bytes/px）
                totalOriginalBytes += (long)original * original * 4;
                totalOptimizedBytes += (long)optimized * optimized * 4;
                textureCount++;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("最適化サマリー", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"対象テクスチャ数: {textureCount}");
            if (totalOriginalBytes > 0)
            {
                float reductionPercent = (1f - (float)totalOptimizedBytes / totalOriginalBytes) * 100f;
                EditorGUILayout.LabelField(
                    $"推定VRAM削減: {FormatBytes(totalOriginalBytes)} → {FormatBytes(totalOptimizedBytes)} ({reductionPercent:F0}% 削減)");
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTextureSizeInfo(Material material)
        {
            if (_sizeCache == null)
                return;

            var textureProps = ShaderPropertyResolver.GetUV0TextureProperties(material);
            foreach (var propName in textureProps)
            {
                var tex = material.GetTexture(propName) as Texture2D;
                if (tex != null && _sizeCache.TryGetValue(tex, out var sizes))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(
                        $"{propName}: {sizes.Original}px → {sizes.Optimized}px",
                        EditorStyles.miniLabel);
                    EditorGUI.indentLevel--;
                }
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024f * 1024f):F1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024f:F1} KB";
            return $"{bytes} B";
        }

        private void DetectTextures(TextureCropSettings settings)
        {
            Undo.RecordObject(settings, "テクスチャを検知");

            var avatarRoot = settings.gameObject;
            var entries = RendererCollector.Collect(avatarRoot);

            // 現在のエントリの設定を保持するためのマップ
            var existingSettings = new Dictionary<Material, MaterialEntry>();
            if (settings.Entries != null)
            {
                foreach (var entry in settings.Entries)
                {
                    if (entry.Material != null)
                        existingSettings[entry.Material] = entry;
                }
            }

            // テクスチャグループを構築（除外なし）
            var textureGroups = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            // サイズキャッシュをリセット
            _sizeCache = new Dictionary<Texture2D, (int, int)>();

            // 1段階以上削減できるマテリアルを判定
            var validMaterials = new HashSet<Material>();
            foreach (var kvp in textureGroups)
            {
                var texture = kvp.Key;
                var group = kvp.Value;

                // UV島検出
                var allIslandBounds = new List<Rect>();
                var processedMeshes = new HashSet<Mesh>();
                bool skip = false;

                foreach (var reference in group.References)
                {
                    if (processedMeshes.Contains(reference.Mesh))
                        continue;
                    processedMeshes.Add(reference.Mesh);

                    var islandBounds = UVIslandDetector.DetectIslandBounds(reference.Mesh);
                    if (islandBounds == null)
                    {
                        skip = true;
                        break;
                    }
                    allIslandBounds.AddRange(islandBounds);
                }

                if (skip || allIslandBounds.Count == 0)
                    continue;

                var usedRect = UVRectCalculator.CalculateUsedRect(allIslandBounds);
                int originalSize = Mathf.Max(texture.width, texture.height);
                int optimizedSize = PowerOfTwoCalculator.Calculate(usedRect, originalSize);

                if (!PowerOfTwoCalculator.IsWorthOptimizing(originalSize, optimizedSize))
                    continue;

                // サイズ情報をキャッシュ
                _sizeCache[texture] = (originalSize, optimizedSize);

                // この条件を満たすマテリアルを有効リストに追加
                foreach (var reference in group.References)
                {
                    validMaterials.Add(reference.Material);
                }
            }

            // 新しいエントリリストを構築（既存設定をマージ）
            var newEntries = new List<MaterialEntry>();
            foreach (var mat in validMaterials)
            {
                if (existingSettings.TryGetValue(mat, out var existing))
                {
                    newEntries.Add(existing);
                }
                else
                {
                    newEntries.Add(new MaterialEntry
                    {
                        Material = mat,
                        Excluded = false,
                        UVRotation = UVRotationMode.Normal
                    });
                }
            }

            settings.Entries = newEntries;
            EditorUtility.SetDirty(settings);
        }
    }
}
