using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// メッシュを複製し、UV0をUsedRectに合わせてリマップするクラス。
    /// </summary>
    public static class MeshRemapper
    {
        /// <summary>
        /// メッシュを複製し、UV0をUsedRectに合わせてリマップした複製を返す。
        /// リマップ式: newUV = (oldUV - usedRect.min) / usedRect.size
        /// </summary>
        public static Mesh Remap(Mesh source, Rect usedRect)
        {
            var copy = Object.Instantiate(source);

            var uvs = copy.uv;
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(
                    (uvs[i].x - usedRect.x) / usedRect.width,
                    (uvs[i].y - usedRect.y) / usedRect.height
                );
            }
            copy.uv = uvs;

            return copy;
        }
    }
}
