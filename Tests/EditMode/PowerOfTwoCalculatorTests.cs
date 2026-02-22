using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class PowerOfTwoCalculatorTests
    {
        // Calculate: UsedRectとオリジナルサイズから必要最小の2のべき乗サイズを算出

        [Test]
        public void Calculate_HalfUsedRect_ReturnsHalfSize()
        {
            // UsedRect covers 50% of UV width -> 4096 * 0.5 = 2048 -> PoT = 2048
            var usedRect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(2048, result);
        }

        [Test]
        public void Calculate_FullUsedRect_ReturnsSameSize()
        {
            var usedRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(4096, result);
        }

        [Test]
        public void Calculate_QuarterUsedRect_ReturnsQuarterSize()
        {
            // UsedRect covers 25% -> 4096 * 0.25 = 1024 -> PoT = 1024
            var usedRect = new Rect(0.0f, 0.0f, 0.25f, 0.25f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(1024, result);
        }

        [Test]
        public void Calculate_SlightlyOverHalf_RoundsUpToNextPoT()
        {
            // UsedRect: width=0.51 -> 4096 * 0.51 = 2089 -> 次のPoT = 4096
            var usedRect = new Rect(0.0f, 0.0f, 0.51f, 0.51f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(4096, result);
        }

        [Test]
        public void Calculate_AsymmetricRect_UsesLargerDimension()
        {
            // Width 0.5, Height 0.25 -> max(4096*0.5, 4096*0.25) = 2048 -> PoT = 2048
            var usedRect = new Rect(0.0f, 0.0f, 0.5f, 0.25f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(2048, result);
        }

        [Test]
        public void Calculate_VerySmall_ReturnsMinTextureSize()
        {
            // UsedRect extremely small -> should not go below MinTextureSize
            var usedRect = new Rect(0.0f, 0.0f, 0.001f, 0.001f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(Constants.MinTextureSize, result);
        }

        [Test]
        public void Calculate_From2048_HalfUsed_Returns1024()
        {
            var usedRect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 2048);
            Assert.AreEqual(1024, result);
        }

        // IsWorthOptimizing: 1段階以上小さくなるか

        [Test]
        public void IsWorthOptimizing_HalfSize_ReturnsTrue()
        {
            Assert.IsTrue(PowerOfTwoCalculator.IsWorthOptimizing(4096, 2048));
        }

        [Test]
        public void IsWorthOptimizing_SameSize_ReturnsFalse()
        {
            Assert.IsFalse(PowerOfTwoCalculator.IsWorthOptimizing(4096, 4096));
        }

        [Test]
        public void IsWorthOptimizing_QuarterSize_ReturnsTrue()
        {
            Assert.IsTrue(PowerOfTwoCalculator.IsWorthOptimizing(4096, 1024));
        }
    }
}
