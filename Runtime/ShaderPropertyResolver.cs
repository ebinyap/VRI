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
        /// UVチャンネル指定プロパティの命名パターン。
        /// Poiyomi: _MainTexUV (値: 0=UV0, 1=UV1, ...)
        /// Liltoon: _Main2ndTex_UVMode (値: 0=UV0, 1=UV1, ...)
        /// </summary>
        private static readonly string[] UVChannelSuffixes = new[]
        {
            "UV",
            "_UVMode",
            "_UV"
        };

        /// <summary>
        /// マテリアルからUV0を使用しているテクスチャプロパティ名のリストを返す。
        /// tiling/offsetがデフォルト以外のプロパティは除外する。
        /// テクスチャが設定されていないプロパティも除外する。
        /// UV0以外が設定されているプロパティも除外する。
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

                // UVチャンネルチェック
                if (!IsUV0(material, propName))
                    continue;

                result.Add(propName);
            }

            return result;
        }

        /// <summary>
        /// 指定テクスチャプロパティがUV0を使用しているかを判定する。
        /// UVチャンネル指定プロパティが存在しない場合はUV0と見なす（Standard等）。
        /// Poiyomi: {prop}UV, Liltoon: {prop}_UVMode のパターンを検出。
        /// </summary>
        internal static bool IsUV0(Material material, string texturePropertyName)
        {
            foreach (var suffix in UVChannelSuffixes)
            {
                var uvPropName = texturePropertyName + suffix;
                if (material.HasProperty(uvPropName))
                {
                    int uvChannel = (int)material.GetFloat(uvPropName);
                    if (uvChannel != 0)
                    {
                        TCOLogger.Info("ShaderPropertyResolver",
                            $"UV0以外のチャンネル({uvChannel})を使用。スキップします",
                            $"{texturePropertyName} ({uvPropName})");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
