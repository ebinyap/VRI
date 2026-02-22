using System.Collections.Generic;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// 複数のAABBリストから全体の和集合Rectを算出するクラス。
    /// </summary>
    public static class UVRectCalculator
    {
        /// <summary>
        /// 複数のAABBリストを受け取り、全体の和集合Rectを返す。
        /// </summary>
        public static Rect CalculateUsedRect(IEnumerable<Rect> islandBounds)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var rect in islandBounds)
            {
                if (rect.xMin < minX) minX = rect.xMin;
                if (rect.yMin < minY) minY = rect.yMin;
                if (rect.xMax > maxX) maxX = rect.xMax;
                if (rect.yMax > maxY) maxY = rect.yMax;
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
