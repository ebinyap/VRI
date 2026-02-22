using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// UsedRectに対応するピクセル領域を新しいテクスチャとして再構成するクラス。
    /// </summary>
    public static class TextureRebuilder
    {
        /// <summary>
        /// UsedRectに対応するピクセル領域を新しいテクスチャとして再構成して返す。
        /// ソーステクスチャからUsedRect範囲のピクセルをコピーし、targetSizeにリサイズする。
        /// </summary>
        public static Texture2D Rebuild(Texture2D source, Rect usedRect, int targetSize)
        {
            int srcX = Mathf.FloorToInt(usedRect.x * source.width);
            int srcY = Mathf.FloorToInt(usedRect.y * source.height);
            int srcW = Mathf.FloorToInt(usedRect.width * source.width);
            int srcH = Mathf.FloorToInt(usedRect.height * source.height);

            // 範囲クランプ
            srcX = Mathf.Clamp(srcX, 0, source.width - 1);
            srcY = Mathf.Clamp(srcY, 0, source.height - 1);
            srcW = Mathf.Clamp(srcW, 1, source.width - srcX);
            srcH = Mathf.Clamp(srcH, 1, source.height - srcY);

            // ソースからUsedRect領域のピクセルを取得
            var sourcePixels = source.GetPixels(srcX, srcY, srcW, srcH);

            // 中間テクスチャにコピー（ソース解像度のUsedRect部分）
            var cropped = new Texture2D(srcW, srcH, source.format, false);
            cropped.SetPixels(sourcePixels);
            cropped.Apply();

            // targetSizeと同じであればそのまま返す
            if (srcW == targetSize && srcH == targetSize)
                return cropped;

            // targetSizeにリサイズ（RenderTextureを使用）
            var rt = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            RenderTexture.active = rt;

            Graphics.Blit(cropped, rt);

            var result = new Texture2D(targetSize, targetSize, source.format, false);
            result.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            Object.DestroyImmediate(cropped);

            return result;
        }
    }
}
