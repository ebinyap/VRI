using System.Collections.Generic;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// Renderer・Mesh・Materialの組を表す構造体。
    /// </summary>
    public readonly struct RendererEntry
    {
        /// <summary>対象のRenderer。</summary>
        public Renderer Renderer { get; }
        /// <summary>対象のMesh。</summary>
        public Mesh Mesh { get; }
        /// <summary>対象のMaterial配列。</summary>
        public Material[] Materials { get; }

        public RendererEntry(Renderer renderer, Mesh mesh, Material[] materials)
        {
            Renderer = renderer;
            Mesh = mesh;
            Materials = materials;
        }
    }

    /// <summary>
    /// Avatar配下の全RendererからRenderer・Mesh・Materialの組を収集するクラス。
    /// </summary>
    public static class RendererCollector
    {
        /// <summary>
        /// Avatar配下の全RendererからRenderer・Mesh・Materialの組を収集して返す。
        /// メッシュが取得できないRendererはスキップする。
        /// </summary>
        public static List<RendererEntry> Collect(GameObject avatarRoot)
        {
            var result = new List<RendererEntry>();
            var renderers = avatarRoot.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                Mesh mesh = GetMesh(renderer);
                if (mesh == null)
                    continue;

                var materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                    continue;

                result.Add(new RendererEntry(renderer, mesh, materials));
            }

            return result;
        }

        private static Mesh GetMesh(Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer smr)
                return smr.sharedMesh;

            if (renderer is MeshRenderer)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                return meshFilter != null ? meshFilter.sharedMesh : null;
            }

            return null;
        }
    }
}
