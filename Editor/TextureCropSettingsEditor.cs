using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
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
        // 検知結果のキャッシュ（テクスチャごとのサイズ情報: 元幅, 元高さ, 最適化幅, 最適化高さ）
        private Dictionary<Texture2D, (int OrigW, int OrigH, int OptW, int OptH)> _sizeCache;

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

                // Undo記録は変更前に行う必要がある
                Undo.RecordObject(settings, "TextureCropSettings変更");

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

            // 除外されていないマテリアルが参照するテクスチャを収集
            var activeTextures = new HashSet<Texture2D>();
            foreach (var entry in settings.Entries)
            {
                if (!entry.Excluded && entry.Material != null)
                {
                    var props = ShaderPropertyResolver.GetUV0TextureProperties(entry.Material);
                    foreach (var propName in props)
                    {
                        var tex = entry.Material.GetTexture(propName) as Texture2D;
                        if (tex != null && _sizeCache.ContainsKey(tex))
                            activeTextures.Add(tex);
                    }
                }
            }

            foreach (var tex in activeTextures)
            {
                var sizes = _sizeCache[tex];
                // ピクセル数ベースの概算VRAM（RGBA 4bytes/px）
                totalOriginalBytes += (long)tex.width * tex.height * 4;
                totalOptimizedBytes += (long)sizes.OptW * sizes.OptH * 4;
                textureCount++;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("最適化サマリー", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"対象テクスチャ数: {textureCount}");
            if (totalOriginalBytes > 0)
            {
                float reductionPercent = (1f - (float)totalOptimizedBytes / totalOriginalBytes) * 100f;
                EditorGUILayout.LabelField(
                    $"推定VRAM削減: {TCOLogger.FormatBytes(totalOriginalBytes)} → {TCOLogger.FormatBytes(totalOptimizedBytes)} ({reductionPercent:F0}% 削減)");
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
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 15f + 16f);

                    // サムネイル（クリックでProjectウィンドウにハイライト）
                    var thumbnailRect = GUILayoutUtility.GetRect(
                        32, 32, GUILayout.Width(32), GUILayout.Height(32));
                    EditorGUI.DrawPreviewTexture(thumbnailRect, tex);
                    if (Event.current.type == EventType.MouseDown &&
                        thumbnailRect.Contains(Event.current.mousePosition))
                    {
                        EditorGUIUtility.PingObject(tex);
                        Selection.activeObject = tex;
                        Event.current.Use();
                    }
                    EditorGUIUtility.AddCursorRect(thumbnailRect, MouseCursor.Link);

                    // テクスチャ名 + サイズ情報
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(tex.name, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(
                        $"{sizes.OrigW}x{sizes.OrigH} → {sizes.OptW}x{sizes.OptH}",
                        EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DetectTextures(TextureCropSettings settings)
        {
            Undo.RecordObject(settings, "テクスチャを検知");

            try
            {
                DetectWithNDMFProcessing(settings);
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"[TextureCropOptimizer] NDMF処理に失敗しました。従来の検知を実行します: {e.Message}");
                DetectWithoutNDMFProcessing(settings);
            }
        }

        /// <summary>
        /// NDMF処理後のクローンを解析してテクスチャを検知する。
        /// AAO/MA/TTT等のメッシュ削除・最適化が反映された状態で解析される。
        /// </summary>
        private void DetectWithNDMFProcessing(TextureCropSettings settings)
        {
            var avatarRoot = settings.gameObject;

            // アバターをクローン
            var clone = UnityEngine.Object.Instantiate(avatarRoot);
            clone.name = avatarRoot.name + "_TCO_Temp";

            try
            {
                // クローンからTextureCropSettingsを除去（TCO自体は実行させない）
                foreach (var tcoSettings in clone.GetComponentsInChildren<TextureCropSettings>(true))
                {
                    UnityEngine.Object.DestroyImmediate(tcoSettings);
                }

                // NDMF処理を実行（AAO/MA/TTT等が適用される。EditorOnlyも除去される）
                AvatarProcessor.ProcessAvatar(clone);

                // 処理後のクローンを解析
                var processedEntries = RendererCollector.Collect(clone);
                var processedGroups = TextureGroupBuilder.Build(
                    processedEntries, new HashSet<Material>());

                // 最適化可能テクスチャを特定（テクスチャはアセット参照なので同一インスタンス）
                var optimizableTextures =
                    new Dictionary<Texture2D, (int OrigW, int OrigH, int OptW, int OptH)>();
                foreach (var kvp in processedGroups)
                {
                    var result = OptimizationPipeline.AnalyzeTextureGroup(kvp.Key, kvp.Value);
                    if (result != null)
                        optimizableTextures[kvp.Key] = (result.OriginalWidth, result.OriginalHeight, result.OptimizedWidth, result.OptimizedHeight);
                }

                // サイズキャッシュ更新
                _sizeCache = new Dictionary<Texture2D, (int, int, int, int)>();
                foreach (var kvp in optimizableTextures)
                    _sizeCache[kvp.Key] = kvp.Value;

                // 元アバターのマテリアルから、最適化対象テクスチャを参照するものを特定
                var originalEntries = RendererCollector.Collect(avatarRoot);
                var validMaterials = new HashSet<Material>();
                foreach (var entry in originalEntries)
                {
                    foreach (var mat in entry.Materials)
                    {
                        if (mat == null) continue;
                        var props = ShaderPropertyResolver.GetUV0TextureProperties(mat);
                        foreach (var propName in props)
                        {
                            var tex = mat.GetTexture(propName) as Texture2D;
                            if (tex != null && optimizableTextures.ContainsKey(tex))
                                validMaterials.Add(mat);
                        }
                    }
                }

                BuildEntryList(settings, validMaterials);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clone);
            }
        }

        /// <summary>
        /// NDMF処理なしの従来の検知ロジック（フォールバック）。
        /// </summary>
        private void DetectWithoutNDMFProcessing(TextureCropSettings settings)
        {
            var avatarRoot = settings.gameObject;
            var entries = RendererCollector.Collect(avatarRoot);
            var textureGroups = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            _sizeCache = new Dictionary<Texture2D, (int, int, int, int)>();

            var validMaterials = new HashSet<Material>();
            foreach (var kvp in textureGroups)
            {
                var result = OptimizationPipeline.AnalyzeTextureGroup(kvp.Key, kvp.Value);
                if (result == null)
                    continue;

                _sizeCache[kvp.Key] = (result.OriginalWidth, result.OriginalHeight, result.OptimizedWidth, result.OptimizedHeight);

                foreach (var reference in kvp.Value.References)
                    validMaterials.Add(reference.Material);
            }

            BuildEntryList(settings, validMaterials);
        }

        /// <summary>
        /// 有効なマテリアルセットからエントリリストを構築する（既存設定マージ）。
        /// </summary>
        private void BuildEntryList(TextureCropSettings settings, HashSet<Material> validMaterials)
        {
            var existingSettings = new Dictionary<Material, MaterialEntry>();
            if (settings.Entries != null)
            {
                foreach (var entry in settings.Entries)
                {
                    if (entry.Material != null)
                        existingSettings[entry.Material] = entry;
                }
            }

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
