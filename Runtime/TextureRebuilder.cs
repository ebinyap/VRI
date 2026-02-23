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
            // RenderTextureを使ってGPU上でクロップ＋リサイズ
            // 圧縮テクスチャ（DXT1/BC7等）にも対応
            var rt = RenderTexture.GetTemporary(targetSize, targetSize, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;

            // UsedRect領域のみをtargetSizeにBlit
            // scale/offsetでUsedRect領域を指定
            Graphics.Blit(source, rt, new Vector2(usedRect.width, usedRect.height),
                new Vector2(usedRect.x, usedRect.y));

            RenderTexture.active = rt;
            var result = new Texture2D(targetSize, targetSize, TextureFormat.RGBA32, false);
            result.name = source.name + "_cropped";
            result.ReadPixels(new Rect(0, 0, targetSize, targetSize), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }
    }
}
