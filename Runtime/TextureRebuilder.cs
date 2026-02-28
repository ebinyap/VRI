using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// UsedRectに対応するピクセル領域を新しいテクスチャとして再構成するクラス。
    /// </summary>
    public static class TextureRebuilder
    {
        /// <summary>
        /// UsedRectに対応するピクセル領域を新しいテクスチャとして再構成して返す。（正方形用・後方互換）
        /// </summary>
        public static Texture2D Rebuild(Texture2D source, Rect usedRect, int targetSize)
        {
            return Rebuild(source, usedRect, targetSize, targetSize);
        }

        /// <summary>
        /// UsedRectに対応するピクセル領域を指定の幅・高さで新しいテクスチャとして再構成して返す。
        /// ソーステクスチャからUsedRect範囲のピクセルをコピーし、targetWidth×targetHeightにリサイズする。
        /// </summary>
        public static Texture2D Rebuild(Texture2D source, Rect usedRect, int targetWidth, int targetHeight)
        {
            // RenderTextureを使ってGPU上でクロップ＋リサイズ
            // 圧縮テクスチャ（DXT1/BC7等）にも対応
            var rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;

            try
            {
                // UsedRect領域のみをtargetWidth×targetHeightにBlit
                // scale/offsetでUsedRect領域を指定
                Graphics.Blit(source, rt, new Vector2(usedRect.width, usedRect.height),
                    new Vector2(usedRect.x, usedRect.y));

                RenderTexture.active = rt;
                var result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
                result.name = source.name + "_cropped";
                result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
                result.Apply();

                return result;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}
