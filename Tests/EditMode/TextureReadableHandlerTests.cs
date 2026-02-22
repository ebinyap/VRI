using System;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class TextureReadableHandlerTests
    {
        private Texture2D _texture;

        [TearDown]
        public void TearDown()
        {
            if (_texture != null)
                Object.DestroyImmediate(_texture);
        }

        [Test]
        public void Constructor_AlreadyReadable_DoesNotThrow()
        {
            // isReadable=trueのテクスチャ → エラーなく生成できる
            _texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            // プログラムで作成したTexture2DはデフォルトでisReadable=true

            Assert.DoesNotThrow(() =>
            {
                using (var handler = new TextureReadableHandler(_texture))
                {
                    Assert.IsTrue(_texture.isReadable);
                }
            });
        }

        [Test]
        public void Dispose_AlreadyReadable_StillReadableAfterDispose()
        {
            _texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);

            using (var handler = new TextureReadableHandler(_texture))
            {
                // using中はreadable
                Assert.IsTrue(_texture.isReadable);
            }

            // 元々readableだったので、Dispose後もreadableのまま
            Assert.IsTrue(_texture.isReadable);
        }

        [Test]
        public void Implements_IDisposable()
        {
            _texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            var handler = new TextureReadableHandler(_texture);
            Assert.IsInstanceOf<IDisposable>(handler);
            handler.Dispose();
        }
    }
}
