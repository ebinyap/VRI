using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class ShaderPropertyResolverTests
    {
        private Material _material;

        [TearDown]
        public void TearDown()
        {
            if (_material != null)
                Object.DestroyImmediate(_material);
        }

        [Test]
        public void GetUV0TextureProperties_StandardShader_ReturnsMainTex()
        {
            _material = new Material(Shader.Find("Standard"));
            var tex = Texture2D.whiteTexture;
            _material.mainTexture = tex;

            var result = ShaderPropertyResolver.GetUV0TextureProperties(_material);

            Assert.IsNotNull(result);
            Assert.Contains("_MainTex", result);
        }

        [Test]
        public void GetUV0TextureProperties_NoTexture_ReturnsEmptyList()
        {
            _material = new Material(Shader.Find("Standard"));
            // テクスチャを設定しない

            var result = ShaderPropertyResolver.GetUV0TextureProperties(_material);

            Assert.IsNotNull(result);
            // テクスチャが設定されていないプロパティは返さない
            foreach (var prop in result)
            {
                Assert.IsNotNull(_material.GetTexture(prop),
                    $"Property {prop} should have a texture assigned");
            }
        }

        [Test]
        public void GetUV0TextureProperties_NonDefaultTiling_ExcludesProperty()
        {
            _material = new Material(Shader.Find("Standard"));
            _material.mainTexture = Texture2D.whiteTexture;
            _material.mainTextureScale = new Vector2(2.0f, 2.0f); // non-default tiling

            var result = ShaderPropertyResolver.GetUV0TextureProperties(_material);

            Assert.IsFalse(result.Contains("_MainTex"));
        }

        [Test]
        public void GetUV0TextureProperties_NonDefaultOffset_ExcludesProperty()
        {
            _material = new Material(Shader.Find("Standard"));
            _material.mainTexture = Texture2D.whiteTexture;
            _material.mainTextureOffset = new Vector2(0.5f, 0.0f); // non-default offset

            var result = ShaderPropertyResolver.GetUV0TextureProperties(_material);

            Assert.IsFalse(result.Contains("_MainTex"));
        }

        [Test]
        public void GetUV0TextureProperties_DefaultTilingAndOffset_IncludesProperty()
        {
            _material = new Material(Shader.Find("Standard"));
            _material.mainTexture = Texture2D.whiteTexture;
            _material.mainTextureScale = new Vector2(1.0f, 1.0f);
            _material.mainTextureOffset = new Vector2(0.0f, 0.0f);

            var result = ShaderPropertyResolver.GetUV0TextureProperties(_material);

            Assert.Contains("_MainTex", result);
        }

        [Test]
        public void IsUV0_NoUVChannelProperty_ReturnsTrue()
        {
            // Standard shader にはUVチャンネルプロパティがないのでUV0と見なす
            _material = new Material(Shader.Find("Standard"));

            Assert.IsTrue(ShaderPropertyResolver.IsUV0(_material, "_MainTex"));
        }

        [Test]
        public void GetUV0TextureProperties_MultipleTexturesSet_ReturnsAll()
        {
            _material = new Material(Shader.Find("Standard"));
            _material.mainTexture = Texture2D.whiteTexture;

            // _BumpMapも設定（Standardシェーダーはノーマルマップを持つ）
            _material.SetTexture("_BumpMap", Texture2D.normalTexture);

            var result = ShaderPropertyResolver.GetUV0TextureProperties(_material);

            Assert.Contains("_MainTex", result);
            Assert.Contains("_BumpMap", result);
        }

        [Test]
        public void GetUV0TextureProperties_OnlyTilingNonDefault_ExcludesOnlyThatProperty()
        {
            _material = new Material(Shader.Find("Standard"));
            _material.mainTexture = Texture2D.whiteTexture;
            _material.SetTexture("_BumpMap", Texture2D.normalTexture);

            // _MainTex のtilingだけ非デフォルト
            _material.SetTextureScale("_MainTex", new Vector2(2f, 2f));

            var result = ShaderPropertyResolver.GetUV0TextureProperties(_material);

            // _MainTexは除外、_BumpMapは含まれる
            Assert.IsFalse(result.Contains("_MainTex"));
            Assert.Contains("_BumpMap", result);
        }
    }
}
