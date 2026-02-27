using NUnit.Framework;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class ConstantsTests
    {
        [Test]
        public void MenuPath_HasExpectedValue()
        {
            Assert.AreEqual(
                "GameObject/TextureCropOptimizer/TextureCropSettings",
                Constants.MenuPath);
        }

        [Test]
        public void LogPrefix_HasExpectedValue()
        {
            Assert.AreEqual("[TextureCropOptimizer]", Constants.LogPrefix);
        }

        [Test]
        public void DefaultTiling_IsOne()
        {
            Assert.AreEqual(1.0f, Constants.DefaultTiling);
        }

        [Test]
        public void DefaultOffset_IsZero()
        {
            Assert.AreEqual(0.0f, Constants.DefaultOffset);
        }

        [Test]
        public void MinTextureSize_IsFour()
        {
            Assert.AreEqual(4, Constants.MinTextureSize);
        }

        [Test]
        public void AddComponentMenuPath_DoesNotStartWithGameObject()
        {
            // AddComponentMenu用パスはGameObject/プレフィックスを含まない
            Assert.IsFalse(Constants.AddComponentMenuPath.StartsWith("GameObject/"),
                "AddComponentMenuPathはGameObject/を含まないこと");
        }

        [Test]
        public void AddComponentMenuPath_HasExpectedValue()
        {
            Assert.AreEqual("TextureCropOptimizer/TextureCropSettings",
                Constants.AddComponentMenuPath);
        }
    }
}
