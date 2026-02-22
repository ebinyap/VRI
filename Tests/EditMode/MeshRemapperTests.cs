using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class MeshRemapperTests
    {
        private Mesh _source;
        private Mesh _result;

        [TearDown]
        public void TearDown()
        {
            if (_source != null) Object.DestroyImmediate(_source);
            if (_result != null) Object.DestroyImmediate(_result);
        }

        [Test]
        public void Remap_ReturnsNewMeshInstance()
        {
            _source = CreateQuadMesh(
                new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.0f),
                new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.5f));

            var usedRect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
            _result = MeshRemapper.Remap(_source, usedRect);

            Assert.AreNotSame(_source, _result);
        }

        [Test]
        public void Remap_PreservesVertexCount()
        {
            _source = CreateQuadMesh(
                new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.0f),
                new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.5f));

            _result = MeshRemapper.Remap(_source, new Rect(0.0f, 0.0f, 0.5f, 0.5f));

            Assert.AreEqual(_source.vertexCount, _result.vertexCount);
        }

        [Test]
        public void Remap_PreservesTriangles()
        {
            _source = CreateQuadMesh(
                new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.0f),
                new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.5f));

            _result = MeshRemapper.Remap(_source, new Rect(0.0f, 0.0f, 0.5f, 0.5f));

            Assert.AreEqual(_source.triangles, _result.triangles);
        }

        [Test]
        public void Remap_HalfUsedRect_RemapsUVToFullRange()
        {
            // UV: (0,0)-(0.5,0.5) の領域を使用 → リマップ後は (0,0)-(1,1)
            _source = CreateQuadMesh(
                new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.0f),
                new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.5f));

            _result = MeshRemapper.Remap(_source, new Rect(0.0f, 0.0f, 0.5f, 0.5f));

            var uvs = _result.uv;
            Assert.AreEqual(0.0f, uvs[0].x, 0.001f);
            Assert.AreEqual(0.0f, uvs[0].y, 0.001f);
            Assert.AreEqual(1.0f, uvs[1].x, 0.001f);
            Assert.AreEqual(0.0f, uvs[1].y, 0.001f);
            Assert.AreEqual(0.0f, uvs[2].x, 0.001f);
            Assert.AreEqual(1.0f, uvs[2].y, 0.001f);
            Assert.AreEqual(1.0f, uvs[3].x, 0.001f);
            Assert.AreEqual(1.0f, uvs[3].y, 0.001f);
        }

        [Test]
        public void Remap_OffsetUsedRect_RemapsCorrectly()
        {
            // UV: (0.25,0.25)-(0.75,0.75) → usedRect=(0.25,0.25,0.5,0.5)
            // リマップ: (u - 0.25) / 0.5 → (0.25→0, 0.75→1)
            _source = CreateQuadMesh(
                new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.25f),
                new Vector2(0.25f, 0.75f), new Vector2(0.75f, 0.75f));

            _result = MeshRemapper.Remap(_source, new Rect(0.25f, 0.25f, 0.5f, 0.5f));

            var uvs = _result.uv;
            Assert.AreEqual(0.0f, uvs[0].x, 0.001f);
            Assert.AreEqual(0.0f, uvs[0].y, 0.001f);
            Assert.AreEqual(1.0f, uvs[1].x, 0.001f);
            Assert.AreEqual(0.0f, uvs[1].y, 0.001f);
            Assert.AreEqual(0.0f, uvs[2].x, 0.001f);
            Assert.AreEqual(1.0f, uvs[2].y, 0.001f);
            Assert.AreEqual(1.0f, uvs[3].x, 0.001f);
            Assert.AreEqual(1.0f, uvs[3].y, 0.001f);
        }

        [Test]
        public void Remap_DoesNotModifySource()
        {
            _source = CreateQuadMesh(
                new Vector2(0.1f, 0.2f), new Vector2(0.5f, 0.2f),
                new Vector2(0.1f, 0.5f), new Vector2(0.5f, 0.5f));
            var originalUVs = _source.uv;

            _result = MeshRemapper.Remap(_source, new Rect(0.1f, 0.2f, 0.4f, 0.3f));

            var sourceUVs = _source.uv;
            for (int i = 0; i < originalUVs.Length; i++)
            {
                Assert.AreEqual(originalUVs[i].x, sourceUVs[i].x, 0.001f);
                Assert.AreEqual(originalUVs[i].y, sourceUVs[i].y, 0.001f);
            }
        }

        private Mesh CreateQuadMesh(Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(0, 0, 0), new Vector3(1, 0, 0),
                new Vector3(0, 1, 0), new Vector3(1, 1, 0)
            };
            mesh.triangles = new[] { 0, 1, 2, 1, 3, 2 };
            mesh.uv = new[] { uv0, uv1, uv2, uv3 };
            return mesh;
        }
    }
}
