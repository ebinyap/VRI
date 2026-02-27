using System.Collections.Generic;
using UnityEngine;

namespace TextureCropOptimizer
{
    /// <summary>
    /// メッシュのUV0からUV島を検出し、各島のAABBを算出するクラス。
    /// </summary>
    public static class UVIslandDetector
    {
        /// <summary>
        /// メッシュのUV0からUV島を検出し、各島のAABBリストを返す。
        /// 0–1範囲外の頂点が存在する場合はnullを返す。
        /// UVが設定されていない場合もnullを返す。
        /// </summary>
        public static List<Rect> DetectIslandBounds(Mesh mesh)
        {
            var uvs = mesh.uv;
            if (uvs == null || uvs.Length == 0)
                return null;

            // 0-1範囲チェック
            for (int i = 0; i < uvs.Length; i++)
            {
                if (uvs[i].x < 0f || uvs[i].x > 1f || uvs[i].y < 0f || uvs[i].y > 1f)
                    return null;
            }

            var triangles = mesh.triangles;
            if (triangles.Length == 0)
                return new List<Rect>();

            // Union-Find
            var parent = new int[uvs.Length];
            var rank = new int[uvs.Length];
            for (int i = 0; i < parent.Length; i++)
                parent[i] = i;

            // 三角形の頂点を結合し、使用頂点を記録
            var usedVertices = new HashSet<int>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v0 = triangles[i], v1 = triangles[i + 1], v2 = triangles[i + 2];
                usedVertices.Add(v0);
                usedVertices.Add(v1);
                usedVertices.Add(v2);
                Union(parent, rank, v0, v1);
                Union(parent, rank, v0, v2);
            }

            // 島ごとにグループ化
            var islands = new Dictionary<int, List<int>>();
            foreach (int v in usedVertices)
            {
                int root = Find(parent, v);
                if (!islands.TryGetValue(root, out var list))
                {
                    list = new List<int>();
                    islands[root] = list;
                }
                list.Add(v);
            }

            // 各島のAABBを算出
            var result = new List<Rect>(islands.Count);
            foreach (var kvp in islands)
            {
                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;

                foreach (int idx in kvp.Value)
                {
                    var uv = uvs[idx];
                    if (uv.x < minX) minX = uv.x;
                    if (uv.y < minY) minY = uv.y;
                    if (uv.x > maxX) maxX = uv.x;
                    if (uv.y > maxY) maxY = uv.y;
                }

                result.Add(new Rect(minX, minY, maxX - minX, maxY - minY));
            }

            return result;
        }

        private static int Find(int[] parent, int x)
        {
            while (parent[x] != x)
            {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }
            return x;
        }

        private static void Union(int[] parent, int[] rank, int a, int b)
        {
            int ra = Find(parent, a), rb = Find(parent, b);
            if (ra == rb) return;
            if (rank[ra] < rank[rb]) { int t = ra; ra = rb; rb = t; }
            parent[rb] = ra;
            if (rank[ra] == rank[rb]) rank[ra]++;
        }
    }
}
