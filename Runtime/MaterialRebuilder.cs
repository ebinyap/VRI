using System.Collections.Generic;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// マテリアルを複製し、指定プロパティのテクスチャを差し替えるクラス。
    /// </summary>
    public static class MaterialRebuilder
    {
        /// <summary>
        /// マテリアルを複製し、指定プロパティのテクスチャを差し替えた複製を返す。
        /// </summary>
        public static Material Rebuild(Material source, Dictionary<string, Texture2D> textureMap)
        {
            var copy = Object.Instantiate(source);

            foreach (var kvp in textureMap)
            {
                copy.SetTexture(kvp.Key, kvp.Value);
            }

            return copy;
        }
    }
}
