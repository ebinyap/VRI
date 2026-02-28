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

        [Test]
        public void Calculate_ExactPoTBoundary_ReturnsExactPoT()
        {
            // width=0.5 -> 4096 * 0.5 = 2048 (exactly PoT)
            var usedRect = new Rect(0.25f, 0.25f, 0.5f, 0.5f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(2048, result);
        }

        [Test]
        public void Calculate_OneEighth_Returns512()
        {
            var usedRect = new Rect(0.0f, 0.0f, 0.125f, 0.125f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(512, result);
        }

        [Test]
        public void Calculate_SmallOriginal_1024_HalfUsed_Returns512()
        {
            var usedRect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 1024);
            Assert.AreEqual(512, result);
        }

        [Test]
        public void Calculate_WidthDominant_UsesWidth()
        {
            // width=0.5, height=0.1 -> max=0.5 -> 2048
            var usedRect = new Rect(0.0f, 0.0f, 0.5f, 0.1f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(2048, result);
        }

        [Test]
        public void Calculate_HeightDominant_UsesHeight()
        {
            // width=0.1, height=0.5 -> max=0.5 -> 2048
            var usedRect = new Rect(0.0f, 0.0f, 0.1f, 0.5f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(2048, result);
        }

        [Test]
        public void Calculate_ZeroSizeRect_ReturnsMinTextureSize()
        {
            // UsedRect幅・高さが0の場合 → MinTextureSizeを返す
            var usedRect = new Rect(0.5f, 0.5f, 0.0f, 0.0f);
            var result = PowerOfTwoCalculator.Calculate(usedRect, 4096);
            Assert.AreEqual(Constants.MinTextureSize, result);
        }

        // --- CalculateAxis: 軸ごとのPoT計算 ---

        [Test]
        public void CalculateAxis_HalfExtent_ReturnsHalfSize()
        {
            // extent=0.5, 4096 → 2048
            var result = PowerOfTwoCalculator.CalculateAxis(0.5f, 4096);
            Assert.AreEqual(2048, result);
        }

        [Test]
        public void CalculateAxis_FullExtent_ReturnsSameSize()
        {
            var result = PowerOfTwoCalculator.CalculateAxis(1.0f, 4096);
            Assert.AreEqual(4096, result);
        }

        [Test]
        public void CalculateAxis_QuarterExtent_ReturnsQuarterSize()
        {
            var result = PowerOfTwoCalculator.CalculateAxis(0.25f, 4096);
            Assert.AreEqual(1024, result);
        }

        [Test]
        public void CalculateAxis_SlightlyOverHalf_RoundsToNextPoT()
        {
            // 0.51 * 4096 = 2089 → 次のPoT = 4096
            var result = PowerOfTwoCalculator.CalculateAxis(0.51f, 4096);
            Assert.AreEqual(4096, result);
        }

        [Test]
        public void CalculateAxis_ZeroExtent_ReturnsMinSize()
        {
            var result = PowerOfTwoCalculator.CalculateAxis(0.0f, 4096);
            Assert.AreEqual(Constants.MinTextureSize, result);
        }

        [Test]
        public void CalculateAxis_SmallDimension_256()
        {
            // extent=0.5, 256 → 128
            var result = PowerOfTwoCalculator.CalculateAxis(0.5f, 256);
            Assert.AreEqual(128, result);
        }

        [Test]
        public void CalculateAxis_NonSquareTexture_WidthFull_HeightHalf()
        {
            // AAOケース: 256x1024テクスチャ
            // 幅: extent=1.0, 256 → 256
            // 高さ: extent=0.5, 1024 → 512
            var widthResult = PowerOfTwoCalculator.CalculateAxis(1.0f, 256);
            var heightResult = PowerOfTwoCalculator.CalculateAxis(0.5f, 1024);
            Assert.AreEqual(256, widthResult);
            Assert.AreEqual(512, heightResult);
        }

        [Test]
        public void CalculateAxis_OneEighth_Returns512()
        {
            var result = PowerOfTwoCalculator.CalculateAxis(0.125f, 4096);
            Assert.AreEqual(512, result);
        }

        // --- IsWorthOptimizing: 幅・高さ独立判定 ---

        [Test]
        public void IsWorthOptimizing_WidthHeight_WidthReduced_ReturnsTrue()
        {
            Assert.IsTrue(PowerOfTwoCalculator.IsWorthOptimizing(4096, 4096, 2048, 4096));
        }

        [Test]
        public void IsWorthOptimizing_WidthHeight_HeightReduced_ReturnsTrue()
        {
            // AAOケース: 256x1024 → 256x512
            Assert.IsTrue(PowerOfTwoCalculator.IsWorthOptimizing(256, 1024, 256, 512));
        }

        [Test]
        public void IsWorthOptimizing_WidthHeight_BothReduced_ReturnsTrue()
        {
            Assert.IsTrue(PowerOfTwoCalculator.IsWorthOptimizing(4096, 4096, 2048, 2048));
        }

        [Test]
        public void IsWorthOptimizing_WidthHeight_NeitherReduced_ReturnsFalse()
        {
            Assert.IsFalse(PowerOfTwoCalculator.IsWorthOptimizing(256, 1024, 256, 1024));
        }
    }
}
