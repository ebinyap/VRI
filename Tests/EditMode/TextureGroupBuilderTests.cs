using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class TextureGroupBuilderTests
    {
        private Texture2D _texA;
        private Texture2D _texB;
        private Material _matA;
        private Material _matB;
        private Mesh _meshA;
        private Mesh _meshB;
        private GameObject _goA;
        private GameObject _goB;

        [SetUp]
        public void SetUp()
        {
            _texA = new Texture2D(8, 8);
            _texB = new Texture2D(16, 16);
            _matA = new Material(Shader.Find("Standard"));
            _matA.mainTexture = _texA;
            _matB = new Material(Shader.Find("Standard"));
            _matB.mainTexture = _texB;
            _meshA = CreateSimpleMesh();
            _meshB = CreateSimpleMesh();

            _goA = new GameObject("A");
            var mfA = _goA.AddComponent<MeshFilter>();
            mfA.sharedMesh = _meshA;
            var mrA = _goA.AddComponent<MeshRenderer>();
            mrA.sharedMaterials = new[] { _matA };

            _goB = new GameObject("B");
            var mfB = _goB.AddComponent<MeshFilter>();
            mfB.sharedMesh = _meshB;
            var mrB = _goB.AddComponent<MeshRenderer>();
            mrB.sharedMaterials = new[] { _matB };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_texA);
            Object.DestroyImmediate(_texB);
            Object.DestroyImmediate(_matA);
            Object.DestroyImmediate(_matB);
            Object.DestroyImmediate(_meshA);
            Object.DestroyImmediate(_meshB);
            Object.DestroyImmediate(_goA);
            Object.DestroyImmediate(_goB);
        }

        [Test]
        public void Build_TwoDifferentTextures_ReturnsTwoGroups()
        {
            var entries = new List<RendererEntry>
            {
                new RendererEntry(_goA.GetComponent<Renderer>(), _meshA, new[] { _matA }),
                new RendererEntry(_goB.GetComponent<Renderer>(), _meshB, new[] { _matB })
            };

            var result = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey(_texA));
            Assert.IsTrue(result.ContainsKey(_texB));
        }

        [Test]
        public void Build_SameTexture_GroupedTogether()
        {
            // 両方のマテリアルが同じテクスチャを参照
            _matB.mainTexture = _texA;

            var entries = new List<RendererEntry>
            {
                new RendererEntry(_goA.GetComponent<Renderer>(), _meshA, new[] { _matA }),
                new RendererEntry(_goB.GetComponent<Renderer>(), _meshB, new[] { _matB })
            };

            var result = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey(_texA));
            Assert.AreEqual(2, result[_texA].References.Count);
        }

        [Test]
        public void Build_ExcludedMaterial_SkipsIt()
        {
            var excluded = new HashSet<Material> { _matA };

            var entries = new List<RendererEntry>
            {
                new RendererEntry(_goA.GetComponent<Renderer>(), _meshA, new[] { _matA }),
                new RendererEntry(_goB.GetComponent<Renderer>(), _meshB, new[] { _matB })
            };

            var result = TextureGroupBuilder.Build(entries, excluded);

            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result.ContainsKey(_texA));
            Assert.IsTrue(result.ContainsKey(_texB));
        }

        [Test]
        public void Build_AllExcluded_ReturnsEmptyDictionary()
        {
            var excluded = new HashSet<Material> { _matA, _matB };

            var entries = new List<RendererEntry>
            {
                new RendererEntry(_goA.GetComponent<Renderer>(), _meshA, new[] { _matA }),
                new RendererEntry(_goB.GetComponent<Renderer>(), _meshB, new[] { _matB })
            };

            var result = TextureGroupBuilder.Build(entries, excluded);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Build_EmptyEntries_ReturnsEmptyDictionary()
        {
            var result = TextureGroupBuilder.Build(
                new List<RendererEntry>(),
                new HashSet<Material>());

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Build_NonDefaultTiling_SkipsTexture()
        {
            // tiling が (2,2) → ShaderPropertyResolver が除外 → グループに含まれない
            _matA.SetTextureScale("_MainTex", new Vector2(2f, 2f));

            var entries = new List<RendererEntry>
            {
                new RendererEntry(_goA.GetComponent<Renderer>(), _meshA, new[] { _matA })
            };

            var result = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Build_NonDefaultOffset_SkipsTexture()
        {
            // offset が (0.5, 0) → ShaderPropertyResolver が除外 → グループに含まれない
            _matA.SetTextureOffset("_MainTex", new Vector2(0.5f, 0f));

            var entries = new List<RendererEntry>
            {
                new RendererEntry(_goA.GetComponent<Renderer>(), _meshA, new[] { _matA })
            };

            var result = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Build_NullMaterialInArray_SkipsIt()
        {
            // マテリアル配列にnullが含まれている場合
            var entries = new List<RendererEntry>
            {
                new RendererEntry(_goA.GetComponent<Renderer>(), _meshA, new Material[] { null, _matA })
            };

            var result = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            // nullはスキップされ、_matA のテクスチャのみグループ化される
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.ContainsKey(_texA));
        }

        [Test]
        public void Build_MaterialWithNoTexture_ReturnsEmptyGroups()
        {
            // テクスチャ未設定のマテリアル
            var emptyMat = new Material(Shader.Find("Standard"));

            var go = new GameObject("C");
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = _meshA;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = new[] { emptyMat };

            var entries = new List<RendererEntry>
            {
                new RendererEntry(mr, _meshA, new[] { emptyMat })
            };

            var result = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            Assert.AreEqual(0, result.Count);

            Object.DestroyImmediate(emptyMat);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Build_MultipleMaterialsSameRenderer_GroupsByTexture()
        {
            // 同一Rendererに複数マテリアル（異なるテクスチャ）
            var entries = new List<RendererEntry>
            {
                new RendererEntry(_goA.GetComponent<Renderer>(), _meshA, new[] { _matA, _matB })
            };

            var result = TextureGroupBuilder.Build(entries, new HashSet<Material>());

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey(_texA));
            Assert.IsTrue(result.ContainsKey(_texB));
        }

        private Mesh CreateSimpleMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up };
            return mesh;
        }
    }
}
