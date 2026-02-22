using System.Collections.Generic;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// マテリアルからUV0を使用しているテクスチャプロパティ名を解決するクラス。
    /// </summary>
    public static class ShaderPropertyResolver
    {
        /// <summary>
        /// マテリアルからUV0を使用しているテクスチャプロパティ名のリストを返す。
        /// tiling/offsetがデフォルト以外のプロパティは除外する。
        /// テクスチャが設定されていないプロパティも除外する。
        /// </summary>
        public static List<string> GetUV0TextureProperties(Material material)
        {
            var result = new List<string>();
            var propertyNames = material.GetTexturePropertyNames();

            foreach (var propName in propertyNames)
            {
                // テクスチャが設定されていないプロパティはスキップ
                if (material.GetTexture(propName) == null)
                    continue;

                // tiling/offsetチェック
                var scale = material.GetTextureScale(propName);
                var offset = material.GetTextureOffset(propName);

                if (!Mathf.Approximately(scale.x, Constants.DefaultTiling) ||
                    !Mathf.Approximately(scale.y, Constants.DefaultTiling))
                    continue;

                if (!Mathf.Approximately(offset.x, Constants.DefaultOffset) ||
                    !Mathf.Approximately(offset.y, Constants.DefaultOffset))
                    continue;

                result.Add(propName);
            }

            return result;
        }
    }
}
