using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class MaterialRebuilderTests
    {
        private Material _source;
        private Material _result;
        private Texture2D _originalTex;
        private Texture2D _replacementTex;

        [SetUp]
        public void SetUp()
        {
            _originalTex = new Texture2D(8, 8);
            _replacementTex = new Texture2D(4, 4);
        }

        [TearDown]
        public void TearDown()
        {
            if (_source != null) Object.DestroyImmediate(_source);
            if (_result != null) Object.DestroyImmediate(_result);
            if (_originalTex != null) Object.DestroyImmediate(_originalTex);
            if (_replacementTex != null) Object.DestroyImmediate(_replacementTex);
        }

        [Test]
        public void Rebuild_ReturnsNewMaterialInstance()
        {
            _source = new Material(Shader.Find("Standard"));
            _source.mainTexture = _originalTex;

            var textureMap = new Dictionary<string, Texture2D>
            {
                { "_MainTex", _replacementTex }
            };

            _result = MaterialRebuilder.Rebuild(_source, textureMap);

            Assert.AreNotSame(_source, _result);
        }

        [Test]
        public void Rebuild_ReplacesSpecifiedTexture()
        {
            _source = new Material(Shader.Find("Standard"));
            _source.mainTexture = _originalTex;

            var textureMap = new Dictionary<string, Texture2D>
            {
                { "_MainTex", _replacementTex }
            };

            _result = MaterialRebuilder.Rebuild(_source, textureMap);

            Assert.AreSame(_replacementTex, _result.GetTexture("_MainTex"));
        }

        [Test]
        public void Rebuild_DoesNotModifySource()
        {
            _source = new Material(Shader.Find("Standard"));
            _source.mainTexture = _originalTex;

            var textureMap = new Dictionary<string, Texture2D>
            {
                { "_MainTex", _replacementTex }
            };

            _result = MaterialRebuilder.Rebuild(_source, textureMap);

            Assert.AreSame(_originalTex, _source.GetTexture("_MainTex"));
        }

        [Test]
        public void Rebuild_PreservesShader()
        {
            _source = new Material(Shader.Find("Standard"));
            _source.mainTexture = _originalTex;

            var textureMap = new Dictionary<string, Texture2D>
            {
                { "_MainTex", _replacementTex }
            };

            _result = MaterialRebuilder.Rebuild(_source, textureMap);

            Assert.AreEqual(_source.shader, _result.shader);
        }

        [Test]
        public void Rebuild_EmptyTextureMap_ReturnsUnmodifiedCopy()
        {
            _source = new Material(Shader.Find("Standard"));
            _source.mainTexture = _originalTex;

            var textureMap = new Dictionary<string, Texture2D>();

            _result = MaterialRebuilder.Rebuild(_source, textureMap);

            Assert.AreSame(_originalTex, _result.GetTexture("_MainTex"));
        }

        [Test]
        public void Rebuild_SetsNameWithOptimizedSuffix()
        {
            _source = new Material(Shader.Find("Standard"));
            _source.name = "TestMaterial";

            _result = MaterialRebuilder.Rebuild(_source, new Dictionary<string, Texture2D>());

            Assert.AreEqual("TestMaterial_optimized", _result.name);
        }

        [Test]
        public void Rebuild_MultipleTextures_ReplacesAll()
        {
            _source = new Material(Shader.Find("Standard"));
            _source.mainTexture = _originalTex;
            _source.SetTexture("_BumpMap", _originalTex);

            var replacement2 = new Texture2D(2, 2);
            var textureMap = new Dictionary<string, Texture2D>
            {
                { "_MainTex", _replacementTex },
                { "_BumpMap", replacement2 }
            };

            _result = MaterialRebuilder.Rebuild(_source, textureMap);

            Assert.AreSame(_replacementTex, _result.GetTexture("_MainTex"));
            Assert.AreSame(replacement2, _result.GetTexture("_BumpMap"));

            Object.DestroyImmediate(replacement2);
        }
    }
}
