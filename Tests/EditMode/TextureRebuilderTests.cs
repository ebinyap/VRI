using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class TextureRebuilderTests
    {
        private Texture2D _source;
        private Texture2D _result;

        [TearDown]
        public void TearDown()
        {
            if (_source != null) Object.DestroyImmediate(_source);
            if (_result != null) Object.DestroyImmediate(_result);
        }

        [Test]
        public void Rebuild_ReturnsTextureOfTargetSize()
        {
            _source = CreateTestTexture(8, 8);
            var usedRect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);

            _result = TextureRebuilder.Rebuild(_source, usedRect, 4);

            Assert.AreEqual(4, _result.width);
            Assert.AreEqual(4, _result.height);
        }

        [Test]
        public void Rebuild_CopiesCorrectPixelRegion()
        {
            // 8x8テクスチャ、左下4x4が赤、それ以外が青
            _source = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var pixels = new Color[64];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    pixels[y * 8 + x] = (x < 4 && y < 4) ? Color.red : Color.blue;
                }
            }
            _source.SetPixels(pixels);
            _source.Apply();

            // UsedRect: 左下半分 (0,0)-(0.5,0.5)
            var usedRect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
            _result = TextureRebuilder.Rebuild(_source, usedRect, 4);

            // 結果は4x4で全ピクセルが赤のはず
            var resultPixels = _result.GetPixels();
            foreach (var pixel in resultPixels)
            {
                Assert.AreEqual(Color.red.r, pixel.r, 0.01f);
                Assert.AreEqual(Color.red.g, pixel.g, 0.01f);
                Assert.AreEqual(Color.red.b, pixel.b, 0.01f);
            }
        }

        [Test]
        public void Rebuild_OffsetUsedRect_CopiesCorrectRegion()
        {
            // 8x8テクスチャ、右上4x4が緑
            _source = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var pixels = new Color[64];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    pixels[y * 8 + x] = (x >= 4 && y >= 4) ? Color.green : Color.black;
                }
            }
            _source.SetPixels(pixels);
            _source.Apply();

            // UsedRect: 右上 (0.5,0.5)-(1.0,1.0) → width=0.5, height=0.5
            var usedRect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            _result = TextureRebuilder.Rebuild(_source, usedRect, 4);

            // 結果は4x4で全ピクセルが緑のはず
            var resultPixels = _result.GetPixels();
            foreach (var pixel in resultPixels)
            {
                Assert.AreEqual(Color.green.r, pixel.r, 0.01f);
                Assert.AreEqual(Color.green.g, pixel.g, 0.01f);
                Assert.AreEqual(Color.green.b, pixel.b, 0.01f);
            }
        }

        [Test]
        public void Rebuild_OutputFormatIsRGBA32()
        {
            _source = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            FillTexture(_source, Color.white);

            _result = TextureRebuilder.Rebuild(_source, new Rect(0, 0, 1, 1), 8);

            Assert.AreEqual(TextureFormat.RGBA32, _result.format);
        }

        [Test]
        public void Rebuild_SetsNameWithCroppedSuffix()
        {
            _source = CreateTestTexture(8, 8);
            _source.name = "MyTexture";

            _result = TextureRebuilder.Rebuild(_source, new Rect(0, 0, 0.5f, 0.5f), 4);

            Assert.AreEqual("MyTexture_cropped", _result.name);
        }

        private Texture2D CreateTestTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            FillTexture(tex, Color.white);
            return tex;
        }

        private void FillTexture(Texture2D tex, Color color)
        {
            var pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
        }
    }
}
