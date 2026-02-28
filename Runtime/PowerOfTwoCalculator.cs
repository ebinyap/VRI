using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// UsedRectとオリジナルサイズから必要最小の2のべき乗サイズを算出するクラス。
    /// </summary>
    public static class PowerOfTwoCalculator
    {
        /// <summary>
        /// UsedRectとオリジナルサイズから必要最小の2のべき乗サイズを算出する。
        /// 幅・高さの大きい方を基準にし、MinTextureSize未満にはならない。
        /// </summary>
        public static int Calculate(Rect usedRect, int originalSize)
        {
            float maxExtent = Mathf.Max(usedRect.width, usedRect.height);
            int requiredPixels = Mathf.CeilToInt(maxExtent * originalSize);

            if (requiredPixels <= 0)
                return Constants.MinTextureSize;

            int pot = Mathf.NextPowerOfTwo(requiredPixels);

            if (pot > originalSize)
                pot = originalSize;

            if (pot < Constants.MinTextureSize)
                pot = Constants.MinTextureSize;

            return pot;
        }

        /// <summary>
        /// UV使用範囲のextentとオリジナルの辺サイズから、その軸の必要最小2のべき乗サイズを算出する。
        /// 幅・高さを独立して最適化するために使用する。
        /// </summary>
        public static int CalculateAxis(float extent, int originalDimension)
        {
            int requiredPixels = Mathf.CeilToInt(extent * originalDimension);

            if (requiredPixels <= 0)
                return Constants.MinTextureSize;

            int pot = Mathf.NextPowerOfTwo(requiredPixels);

            if (pot > originalDimension)
                pot = originalDimension;

            if (pot < Constants.MinTextureSize)
                pot = Constants.MinTextureSize;

            return pot;
        }

        /// <summary>
        /// 最適化が有効かどうかを判定する（1段階以上小さくなるか）。
        /// </summary>
        public static bool IsWorthOptimizing(int originalSize, int optimizedSize)
        {
            return optimizedSize < originalSize;
        }

        /// <summary>
        /// 幅・高さ独立で最適化が有効かどうかを判定する（いずれかの軸が1段階以上小さくなるか）。
        /// </summary>
        public static bool IsWorthOptimizing(int originalWidth, int originalHeight, int optimizedWidth, int optimizedHeight)
        {
            return optimizedWidth < originalWidth || optimizedHeight < originalHeight;
        }
    }
}
