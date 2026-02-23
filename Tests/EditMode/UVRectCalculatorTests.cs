using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using TextureCropOptimizer;

namespace TextureCropOptimizer.Tests
{
    [TestFixture]
    public class UVRectCalculatorTests
    {
        [Test]
        public void CalculateUsedRect_SingleRect_ReturnsSameRect()
        {
            var rects = new List<Rect> { new Rect(0.1f, 0.2f, 0.3f, 0.4f) };

            var result = UVRectCalculator.CalculateUsedRect(rects);

            Assert.AreEqual(0.1f, result.xMin, 0.001f);
            Assert.AreEqual(0.2f, result.yMin, 0.001f);
            Assert.AreEqual(0.4f, result.xMax, 0.001f);
            Assert.AreEqual(0.6f, result.yMax, 0.001f);
        }

        [Test]
        public void CalculateUsedRect_TwoNonOverlappingRects_ReturnsUnion()
        {
            var rects = new List<Rect>
            {
                new Rect(0.0f, 0.0f, 0.2f, 0.2f),
                new Rect(0.5f, 0.5f, 0.3f, 0.3f)
            };

            var result = UVRectCalculator.CalculateUsedRect(rects);

            Assert.AreEqual(0.0f, result.xMin, 0.001f);
            Assert.AreEqual(0.0f, result.yMin, 0.001f);
            Assert.AreEqual(0.8f, result.xMax, 0.001f);
            Assert.AreEqual(0.8f, result.yMax, 0.001f);
        }

        [Test]
        public void CalculateUsedRect_OverlappingRects_ReturnsUnion()
        {
            var rects = new List<Rect>
            {
                new Rect(0.1f, 0.1f, 0.5f, 0.5f),
                new Rect(0.3f, 0.3f, 0.5f, 0.5f)
            };

            var result = UVRectCalculator.CalculateUsedRect(rects);

            Assert.AreEqual(0.1f, result.xMin, 0.001f);
            Assert.AreEqual(0.1f, result.yMin, 0.001f);
            Assert.AreEqual(0.8f, result.xMax, 0.001f);
            Assert.AreEqual(0.8f, result.yMax, 0.001f);
        }

        [Test]
        public void CalculateUsedRect_FullRange_ReturnsFullUVSpace()
        {
            var rects = new List<Rect>
            {
                new Rect(0.0f, 0.0f, 1.0f, 1.0f)
            };

            var result = UVRectCalculator.CalculateUsedRect(rects);

            Assert.AreEqual(0.0f, result.xMin, 0.001f);
            Assert.AreEqual(0.0f, result.yMin, 0.001f);
            Assert.AreEqual(1.0f, result.xMax, 0.001f);
            Assert.AreEqual(1.0f, result.yMax, 0.001f);
        }

        [Test]
        public void CalculateUsedRect_EmptyList_ReturnsZeroRect()
        {
            var rects = new List<Rect>();

            var result = UVRectCalculator.CalculateUsedRect(rects);

            Assert.AreEqual(0.0f, result.x, 0.001f);
            Assert.AreEqual(0.0f, result.y, 0.001f);
            Assert.AreEqual(0.0f, result.width, 0.001f);
            Assert.AreEqual(0.0f, result.height, 0.001f);
        }

        [Test]
        public void CalculateUsedRect_ThreeRects_ReturnsUnionOfAll()
        {
            var rects = new List<Rect>
            {
                new Rect(0.0f, 0.0f, 0.1f, 0.1f),
                new Rect(0.5f, 0.0f, 0.1f, 0.1f),
                new Rect(0.0f, 0.8f, 0.1f, 0.2f)
            };

            var result = UVRectCalculator.CalculateUsedRect(rects);

            Assert.AreEqual(0.0f, result.xMin, 0.001f);
            Assert.AreEqual(0.0f, result.yMin, 0.001f);
            Assert.AreEqual(0.6f, result.xMax, 0.001f);
            Assert.AreEqual(1.0f, result.yMax, 0.001f);
        }
    }
}
