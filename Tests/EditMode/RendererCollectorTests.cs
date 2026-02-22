using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class RendererCollectorTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);
        }

        [Test]
        public void Collect_EmptyGameObject_ReturnsEmptyList()
        {
            _root = new GameObject("Avatar");

            var result = RendererCollector.Collect(_root);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Collect_SingleMeshRenderer_ReturnsOneEntry()
        {
            _root = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(_root.transform);

            var meshFilter = child.AddComponent<MeshFilter>();
            var mesh = CreateSimpleMesh();
            meshFilter.sharedMesh = mesh;

            var renderer = child.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new[] { mat };

            var result = RendererCollector.Collect(_root);

            Assert.AreEqual(1, result.Count);
            Assert.AreSame(renderer, result[0].Renderer);
            Assert.AreSame(mesh, result[0].Mesh);
            Assert.AreEqual(1, result[0].Materials.Length);
            Assert.AreSame(mat, result[0].Materials[0]);

            Object.DestroyImmediate(mesh);
            Object.DestroyImmediate(mat);
        }

        [Test]
        public void Collect_SkinnedMeshRenderer_ReturnsEntry()
        {
            _root = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(_root.transform);

            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = CreateSimpleMesh();
            smr.sharedMesh = mesh;
            var mat = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new[] { mat };

            var result = RendererCollector.Collect(_root);

            Assert.AreEqual(1, result.Count);
            Assert.AreSame(smr, result[0].Renderer);
            Assert.AreSame(mesh, result[0].Mesh);

            Object.DestroyImmediate(mesh);
            Object.DestroyImmediate(mat);
        }

        [Test]
        public void Collect_NestedChild_CollectsAll()
        {
            _root = new GameObject("Avatar");
            var child1 = new GameObject("Body");
            child1.transform.SetParent(_root.transform);
            var child2 = new GameObject("Hair");
            child2.transform.SetParent(child1.transform);

            SetupMeshRenderer(child1);
            SetupMeshRenderer(child2);

            var result = RendererCollector.Collect(_root);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Collect_NoMesh_SkipsRenderer()
        {
            _root = new GameObject("Avatar");
            var child = new GameObject("Empty");
            child.transform.SetParent(_root.transform);
            child.AddComponent<MeshRenderer>();
            // MeshFilter無し → メッシュなし

            var result = RendererCollector.Collect(_root);

            Assert.AreEqual(0, result.Count);
        }

        private Mesh CreateSimpleMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up };
            return mesh;
        }

        private void SetupMeshRenderer(GameObject go)
        {
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = CreateSimpleMesh();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterials = new[] { new Material(Shader.Find("Standard")) };
        }
    }
}
