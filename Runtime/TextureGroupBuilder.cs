using System.Collections.Generic;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// テクスチャ単位のグループを表すクラス。
    /// </summary>
    public class TextureGroup
    {
        /// <summary>対象のテクスチャ。</summary>
        public Texture2D Texture { get; }
        /// <summary>このテクスチャを参照するMesh・プロパティ名・Materialの組のリスト。</summary>
        public List<(Mesh Mesh, string PropertyName, Material Material)> References { get; }

        public TextureGroup(Texture2D texture)
        {
            Texture = texture;
            References = new List<(Mesh, string, Material)>();
        }
    }

    /// <summary>
    /// RendererEntryリストをテクスチャ単位でグループ化するクラス。
    /// </summary>
    public static class TextureGroupBuilder
    {
        /// <summary>
        /// RendererEntryリストをテクスチャ単位でグループ化して返す。
        /// 除外マテリアルは除く。
        /// </summary>
        public static Dictionary<Texture2D, TextureGroup> Build(
            List<RendererEntry> entries,
            HashSet<Material> excludedMaterials)
        {
            var result = new Dictionary<Texture2D, TextureGroup>();

            foreach (var entry in entries)
            {
                foreach (var material in entry.Materials)
                {
                    if (material == null)
                        continue;

                    if (excludedMaterials.Contains(material))
                        continue;

                    var textureProperties = ShaderPropertyResolver.GetUV0TextureProperties(material);

                    foreach (var propName in textureProperties)
                    {
                        var texture = material.GetTexture(propName) as Texture2D;
                        if (texture == null)
                            continue;

                        if (!result.TryGetValue(texture, out var group))
                        {
                            group = new TextureGroup(texture);
                            result[texture] = group;
                        }

                        group.References.Add((entry.Mesh, propName, material));
                    }
                }
            }

            return result;
        }
    }
}
