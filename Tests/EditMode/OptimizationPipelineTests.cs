using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class OptimizationPipelineTests
    {
        private GameObject _avatar;

        [TearDown]
        public void TearDown()
        {
            if (_avatar != null)
                Object.DestroyImmediate(_avatar);
        }

        [Test]
        public void Execute_NoRenderers_DoesNotThrow()
        {
            _avatar = new GameObject("Avatar");
            var settings = _avatar.AddComponent<TextureCropSettings>();

            Assert.DoesNotThrow(() => OptimizationPipeline.Execute(_avatar, settings));
        }

        [Test]
        public void Execute_ExcludedMaterial_SkipsProcessing()
        {
            _avatar = CreateAvatarWithTexture(8, 8, out var tex, out var mat, out var mesh,
                new[] { new Vector2(0.0f, 0.0f), new Vector2(0.25f, 0.0f), new Vector2(0.0f, 0.25f) });

            var settings = _avatar.AddComponent<TextureCropSettings>();
            settings.Entries.Add(new MaterialEntry
            {
                Material = mat,
                Excluded = true,
                UVRotation = UVRotationMode.Normal
            });

            OptimizationPipeline.Execute(_avatar, settings);

            // 除外されたのでマテリアルのテクスチャは元のまま
            var renderer = _avatar.GetComponentInChildren<Renderer>();
            Assert.AreSame(tex, renderer.sharedMaterial.mainTexture);
        }

        [Test]
        public void Execute_UVOutOfRange_SkipsTexture()
        {
            _avatar = CreateAvatarWithTexture(8, 8, out var tex, out var mat, out var mesh,
                new[] { new Vector2(-0.1f, 0.0f), new Vector2(0.5f, 0.0f), new Vector2(0.0f, 0.5f) });

            var settings = _avatar.AddComponent<TextureCropSettings>();
            settings.Entries.Add(new MaterialEntry
            {
                Material = mat,
                Excluded = false,
                UVRotation = UVRotationMode.Normal
            });

            OptimizationPipeline.Execute(_avatar, settings);

            // UV範囲外なのでスキップ → テクスチャは元のまま
            var renderer = _avatar.GetComponentInChildren<Renderer>();
            Assert.AreSame(tex, renderer.sharedMaterial.mainTexture);
        }

        [Test]
        public void Execute_FullUVRange_NoOptimization_SkipsTexture()
        {
            // UV全範囲を使用 → PoTサイズ変化なし → スキップ
            _avatar = CreateAvatarWithTexture(8, 8, out var tex, out var mat, out var mesh,
                new[] { new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f) });

            var settings = _avatar.AddComponent<TextureCropSettings>();
            settings.Entries.Add(new MaterialEntry
            {
                Material = mat,
                Excluded = false,
                UVRotation = UVRotationMode.Normal
            });

            OptimizationPipeline.Execute(_avatar, settings);

            // 全範囲使用なのでスキップ → テクスチャは元のまま
            var renderer = _avatar.GetComponentInChildren<Renderer>();
            Assert.AreSame(tex, renderer.sharedMaterial.mainTexture);
        }

        [Test]
        public void Execute_SmallUVUsage_OptimizesTexture()
        {
            // UV使用率25%以下 → 8x8のテクスチャが4x4に最適化される
            _avatar = CreateAvatarWithTexture(8, 8, out var tex, out var mat, out var mesh,
                new[] { new Vector2(0.0f, 0.0f), new Vector2(0.25f, 0.0f), new Vector2(0.0f, 0.25f) });

            var settings = _avatar.AddComponent<TextureCropSettings>();

            OptimizationPipeline.Execute(_avatar, settings);

            // テクスチャが差し替えられているはず
            var renderer = _avatar.GetComponentInChildren<Renderer>();
            var newTex = renderer.sharedMaterial.mainTexture as Texture2D;
            Assert.AreNotSame(tex, newTex);
            // 新テクスチャは元テクスチャより小さいはず
            Assert.Less(newTex.width, tex.width);
        }

        [Test]
        public void Execute_EmptySettings_ProcessesAllMaterials()
        {
            // 設定エントリが空（除外なし） → すべてのマテリアルを処理
            _avatar = CreateAvatarWithTexture(8, 8, out var tex, out var mat, out var mesh,
                new[] { new Vector2(0.0f, 0.0f), new Vector2(0.25f, 0.0f), new Vector2(0.0f, 0.25f) });

            var settings = _avatar.AddComponent<TextureCropSettings>();
            // Entriesは空のまま → 除外なし

            OptimizationPipeline.Execute(_avatar, settings);

            var renderer = _avatar.GetComponentInChildren<Renderer>();
            Assert.AreNotSame(tex, renderer.sharedMaterial.mainTexture);
        }

        [Test]
        public void Execute_SmallUVUsage_LogsOptimizationSummary()
        {
            // UV使用率25%以下 → 最適化実行 → サマリーログを出力
            _avatar = CreateAvatarWithTexture(8, 8, out var tex, out var mat, out var mesh,
                new[] { new Vector2(0.0f, 0.0f), new Vector2(0.25f, 0.0f), new Vector2(0.0f, 0.25f) });

            var settings = _avatar.AddComponent<TextureCropSettings>();

            var logs = new List<string>();
            Application.LogCallback handler = (message, stackTrace, type) => logs.Add(message);
            Application.logMessageReceived += handler;
            try
            {
                OptimizationPipeline.Execute(_avatar, settings);
            }
            finally
            {
                Application.logMessageReceived -= handler;
            }

            // サマリーログが出力されていることを検証
            bool hasSummary = logs.Exists(log =>
                log.Contains("最適化サマリー") &&
                log.Contains("テクスチャ: 1") &&
                log.Contains("VRAM削減"));
            Assert.IsTrue(hasSummary, "最適化サマリーログが出力されていません");
        }

        [Test]
        public void Execute_NoOptimization_DoesNotLogSummary()
        {
            // UV全範囲使用 → 最適化なし → サマリーログは出力されない
            _avatar = CreateAvatarWithTexture(8, 8, out var tex, out var mat, out var mesh,
                new[] { new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f) });

            var settings = _avatar.AddComponent<TextureCropSettings>();

            var logs = new List<string>();
            Application.LogCallback handler = (message, stackTrace, type) => logs.Add(message);
            Application.logMessageReceived += handler;
            try
            {
                OptimizationPipeline.Execute(_avatar, settings);
            }
            finally
            {
                Application.logMessageReceived -= handler;
            }

            // "最適化サマリー" を含むログが出力されていないことを確認
            bool hasSummary = logs.Exists(log => log.Contains("最適化サマリー"));
            Assert.IsFalse(hasSummary, "最適化なしの場合サマリーは出力されるべきではない");
        }

        [Test]
        public void Execute_SharedMaterial_BothRenderersGetSameNewMaterial()
        {
            _avatar = new GameObject("Avatar");

            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var pixels = new Color[64];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            var sharedMat = new Material(Shader.Find("Standard"));
            sharedMat.mainTexture = tex;

            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(0.25f, 0), new Vector2(0, 0.25f) };

            // 2つのRendererが同じマテリアルを共有
            var child1 = new GameObject("Body1");
            child1.transform.SetParent(_avatar.transform);
            var mf1 = child1.AddComponent<MeshFilter>();
            mf1.sharedMesh = mesh;
            var mr1 = child1.AddComponent<MeshRenderer>();
            mr1.sharedMaterial = sharedMat;

            var child2 = new GameObject("Body2");
            child2.transform.SetParent(_avatar.transform);
            var mf2 = child2.AddComponent<MeshFilter>();
            mf2.sharedMesh = mesh;
            var mr2 = child2.AddComponent<MeshRenderer>();
            mr2.sharedMaterial = sharedMat;

            var settings = _avatar.AddComponent<TextureCropSettings>();
            OptimizationPipeline.Execute(_avatar, settings);

            // 両方のRendererが同じ新しいマテリアルを参照
            Assert.AreNotSame(sharedMat, mr1.sharedMaterial);
            Assert.AreSame(mr1.sharedMaterial, mr2.sharedMaterial);
        }

        [Test]
        public void Execute_OptimizedTexture_MeshUVsRemapped()
        {
            _avatar = CreateAvatarWithTexture(8, 8, out var tex, out var mat, out var mesh,
                new[] { new Vector2(0.0f, 0.0f), new Vector2(0.25f, 0.0f), new Vector2(0.0f, 0.25f) });

            var settings = _avatar.AddComponent<TextureCropSettings>();
            OptimizationPipeline.Execute(_avatar, settings);

            // メッシュのUVがリマップされている（0-1の全範囲に広がる）
            var mf = _avatar.GetComponentInChildren<MeshFilter>();
            var newUVs = mf.sharedMesh.uv;
            // リマップ後: (0, 0.25) range → (0, 1) range
            Assert.Greater(newUVs[1].x, 0.5f, "リマップ後のUVは0-1範囲に広がるはず");
        }

        // --- AnalyzeTextureGroup 単体テスト ---

        [Test]
        public void AnalyzeTextureGroup_SmallUV_ReturnsResult()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(0.25f, 0), new Vector2(0, 0.25f) };

            var mat = new Material(Shader.Find("Standard"));
            var group = new TextureGroup(tex);
            group.References.Add((mesh, "_MainTex", mat));

            var result = OptimizationPipeline.AnalyzeTextureGroup(tex, group);

            Assert.IsNotNull(result);
            Assert.Less(result.OptimizedSize, result.OriginalSize);

            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(mesh);
            Object.DestroyImmediate(mat);
        }

        [Test]
        public void AnalyzeTextureGroup_FullUV_ReturnsNull()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) };

            var mat = new Material(Shader.Find("Standard"));
            var group = new TextureGroup(tex);
            group.References.Add((mesh, "_MainTex", mat));

            var result = OptimizationPipeline.AnalyzeTextureGroup(tex, group);

            Assert.IsNull(result, "全範囲使用時はnullが返るべき");

            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(mesh);
            Object.DestroyImmediate(mat);
        }

        [Test]
        public void AnalyzeTextureGroup_UVOutOfRange_ReturnsNull()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.uv = new[] { new Vector2(-0.1f, 0), new Vector2(0.5f, 0), new Vector2(0, 0.5f) };

            var mat = new Material(Shader.Find("Standard"));
            var group = new TextureGroup(tex);
            group.References.Add((mesh, "_MainTex", mat));

            var result = OptimizationPipeline.AnalyzeTextureGroup(tex, group);

            Assert.IsNull(result, "UV範囲外の場合はnullが返るべき");

            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(mesh);
            Object.DestroyImmediate(mat);
        }

        private GameObject CreateAvatarWithTexture(int texWidth, int texHeight,
            out Texture2D texture, out Material material, out Mesh mesh, Vector2[] uvs)
        {
            var avatar = new GameObject("Avatar");

            texture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
            var pixels = new Color[texWidth * texHeight];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();

            material = new Material(Shader.Find("Standard"));
            material.mainTexture = texture;

            mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.uv = uvs;

            var child = new GameObject("Body");
            child.transform.SetParent(avatar.transform);
            var mf = child.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = child.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;

            return avatar;
        }
    }
}
