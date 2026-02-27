using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class UVIslandDetectorTests
    {
        private Mesh _mesh;

        [TearDown]
        public void TearDown()
        {
            if (_mesh != null)
                Object.DestroyImmediate(_mesh);
        }

        private Mesh CreateMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs)
        {
            _mesh = new Mesh();
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            if (uvs != null)
                _mesh.uv = uvs;
            return _mesh;
        }

        [Test]
        public void DetectIslandBounds_SingleTriangle_ReturnsOneIsland()
        {
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(0.1f, 0.1f), new Vector2(0.5f, 0.1f), new Vector2(0.1f, 0.5f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0.1f, result[0].xMin, 0.001f);
            Assert.AreEqual(0.1f, result[0].yMin, 0.001f);
            Assert.AreEqual(0.5f, result[0].xMax, 0.001f);
            Assert.AreEqual(0.5f, result[0].yMax, 0.001f);
        }

        [Test]
        public void DetectIslandBounds_TwoSeparateTriangles_ReturnsTwoIslands()
        {
            var mesh = CreateMesh(
                new[]
                {
                    new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0),
                    new Vector3(2, 0, 0), new Vector3(3, 0, 0), new Vector3(2, 1, 0)
                },
                new[] { 0, 1, 2, 3, 4, 5 },
                new[]
                {
                    new Vector2(0.0f, 0.0f), new Vector2(0.2f, 0.0f), new Vector2(0.0f, 0.2f),
                    new Vector2(0.5f, 0.5f), new Vector2(0.8f, 0.5f), new Vector2(0.5f, 0.8f)
                }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void DetectIslandBounds_SharedVertex_MergesIntoOneIsland()
        {
            // Quad: two triangles sharing an edge (vertices 1 and 2)
            var mesh = CreateMesh(
                new[]
                {
                    new Vector3(0, 0, 0), new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0), new Vector3(1, 1, 0)
                },
                new[] { 0, 1, 2, 1, 3, 2 },
                new[]
                {
                    new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.0f),
                    new Vector2(0.0f, 0.5f), new Vector2(0.5f, 0.5f)
                }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0.0f, result[0].xMin, 0.001f);
            Assert.AreEqual(0.0f, result[0].yMin, 0.001f);
            Assert.AreEqual(0.5f, result[0].xMax, 0.001f);
            Assert.AreEqual(0.5f, result[0].yMax, 0.001f);
        }

        [Test]
        public void DetectIslandBounds_UVBelowZero_ReturnsNull()
        {
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(-0.1f, 0.1f), new Vector2(0.5f, 0.1f), new Vector2(0.1f, 0.5f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNull(result);
        }

        [Test]
        public void DetectIslandBounds_UVAboveOne_ReturnsNull()
        {
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(0.1f, 0.1f), new Vector2(1.1f, 0.1f), new Vector2(0.1f, 0.5f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNull(result);
        }

        [Test]
        public void DetectIslandBounds_UVAtBoundary_ReturnsIsland()
        {
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void DetectIslandBounds_NoUV_ReturnsNull()
        {
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                null
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNull(result);
        }

        [Test]
        public void DetectIslandBounds_ThreeIslands_ReturnsThreeRects()
        {
            var mesh = CreateMesh(
                new[]
                {
                    new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0),
                    new Vector3(2, 0, 0), new Vector3(3, 0, 0), new Vector3(2, 1, 0),
                    new Vector3(4, 0, 0), new Vector3(5, 0, 0), new Vector3(4, 1, 0)
                },
                new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                new[]
                {
                    new Vector2(0.0f, 0.0f), new Vector2(0.1f, 0.0f), new Vector2(0.0f, 0.1f),
                    new Vector2(0.3f, 0.3f), new Vector2(0.4f, 0.3f), new Vector2(0.3f, 0.4f),
                    new Vector2(0.7f, 0.7f), new Vector2(0.9f, 0.7f), new Vector2(0.7f, 0.9f)
                }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void DetectIslandBounds_ChainOfTriangles_ReturnsOneIsland()
        {
            // 3つの三角形が鎖状に接続 → 1島
            var mesh = CreateMesh(
                new[]
                {
                    new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0),
                    new Vector3(1.5f, 1, 0), new Vector3(2, 0, 0)
                },
                new[] { 0, 1, 2, 1, 3, 2, 1, 4, 3 },
                new[]
                {
                    new Vector2(0.0f, 0.0f), new Vector2(0.2f, 0.0f), new Vector2(0.1f, 0.2f),
                    new Vector2(0.3f, 0.2f), new Vector2(0.4f, 0.0f)
                }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0.0f, result[0].xMin, 0.001f);
            Assert.AreEqual(0.0f, result[0].yMin, 0.001f);
            Assert.AreEqual(0.4f, result[0].xMax, 0.001f);
            Assert.AreEqual(0.2f, result[0].yMax, 0.001f);
        }

        [Test]
        public void DetectIslandBounds_UVYBelowZero_ReturnsNull()
        {
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(0.1f, -0.1f), new Vector2(0.5f, 0.1f), new Vector2(0.1f, 0.5f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNull(result);
        }

        [Test]
        public void DetectIslandBounds_UVYAboveOne_ReturnsNull()
        {
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(0.1f, 0.1f), new Vector2(0.5f, 0.1f), new Vector2(0.1f, 1.5f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNull(result);
        }

        [Test]
        public void DetectIslandBounds_NoTriangles_ReturnsEmptyList()
        {
            // 頂点はあるが三角形なし → UV島は0個
            _mesh = new Mesh();
            _mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            _mesh.uv = new[] { new Vector2(0.1f, 0.1f), new Vector2(0.5f, 0.1f), new Vector2(0.1f, 0.5f) };
            // trianglesを設定しない

            var result = UVIslandDetector.DetectIslandBounds(_mesh);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void DetectIslandBounds_PointUV_ReturnsSinglePointAABB()
        {
            // 全頂点が同じUV座標 → width=0, height=0 のRect
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0.0f, result[0].width, 0.001f);
            Assert.AreEqual(0.0f, result[0].height, 0.001f);
        }

        [Test]
        public void DetectIslandBounds_SlightlyOverBoundary_TreatsAsValid()
        {
            // 浮動小数点誤差で1.0をわずかに超える場合 → 有効として扱う
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(0.0f, 0.0f), new Vector2(1.00001f, 0.0f), new Vector2(0.0f, 1.0f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result, "微小な浮動小数点誤差はスキップせず有効として扱うべき");
        }

        [Test]
        public void DetectIslandBounds_SlightlyBelowZero_TreatsAsValid()
        {
            // 浮動小数点誤差で0をわずかに下回る場合 → 有効として扱う
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(-0.00001f, 0.1f), new Vector2(0.5f, 0.1f), new Vector2(0.1f, 0.5f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNotNull(result, "微小な浮動小数点誤差はスキップせず有効として扱うべき");
        }

        [Test]
        public void DetectIslandBounds_ClearlyOutOfRange_StillReturnsNull()
        {
            // 明確に範囲外（-0.1以下 or 1.1以上）の場合はnull
            var mesh = CreateMesh(
                new[] { Vector3.zero, Vector3.right, Vector3.up },
                new[] { 0, 1, 2 },
                new[] { new Vector2(-0.1f, 0.1f), new Vector2(0.5f, 0.1f), new Vector2(0.1f, 0.5f) }
            );

            var result = UVIslandDetector.DetectIslandBounds(mesh);

            Assert.IsNull(result, "明確な範囲外はnullを返すべき");
        }
    }
}
