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
            copy.name = source.name + "_remapped";

            var uvs = copy.uv;
            float invWidth = usedRect.width > 0f ? 1f / usedRect.width : 0f;
            float invHeight = usedRect.height > 0f ? 1f / usedRect.height : 0f;
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(
                    (uvs[i].x - usedRect.x) * invWidth,
                    (uvs[i].y - usedRect.y) * invHeight
                );
            }
            copy.uv = uvs;

            return copy;
        }
    }
}
