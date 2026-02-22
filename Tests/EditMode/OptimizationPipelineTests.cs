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
