using System.Collections.Generic;
using System.Linq;

using UnityEngine;


namespace TowerDefense.EditorTools.Tile
{
    // Builds a unit tile block (same bounds as the built-in cube) with a relief top:
    // beveled edges and a sunken path. Top UVs are planar, side faces sample one texel.
    public static class TileMeshBuilder
    {
        private const int pathResolution = 32;


        public static Mesh Build(int bitMaskForPathsConnections, Vector2 sideUV)
        {
            List<float> coords = GridCoords(bitMaskForPathsConnections == TileShape.NoPath ? 1 : pathResolution);
            int count = coords.Count;

            List<Vector3> vertices = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();
            List<int> topTriangles = new();
            List<int> sideTriangles = new();

            for (int j = 0; j < count; j++)
            {
                for (int i = 0; i < count; i++)
                {
                    float u = coords[i];
                    float v = coords[j];
                    vertices.Add(new Vector3(u - 0.5f, 0.5f + TileShape.Height(u, v, bitMaskForPathsConnections), v - 0.5f));
                    normals.Add(Vector3.up);
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int j = 0; j < count - 1; j++)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    int a = j * count + i;
                    int b = a + 1;
                    int d = a + count;
                    int c = d + 1;
                    topTriangles.AddRange(new[] { a, d, c, a, c, b });
                }
            }

            Mesh mesh = new Mesh { name = $"TileBlock_{bitMaskForPathsConnections}" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(topTriangles, 0);
            mesh.RecalculateNormals();

            // Walls are added after normals so the top keeps its smooth shading
            vertices = new List<Vector3>(mesh.vertices);
            normals = new List<Vector3>(mesh.normals);

            for (int k = 0; k < count - 1; k++)
            {
                float from = coords[k];
                float to = coords[k + 1];

                // Corners are given bottom-left, bottom-right as seen from outside the face
                AddWall(new Vector2(1, from), new Vector2(1, to), Vector3.right);
                AddWall(new Vector2(to, 1), new Vector2(from, 1), Vector3.forward);
                AddWall(new Vector2(0, to), new Vector2(0, from), Vector3.left);
                AddWall(new Vector2(from, 0), new Vector2(to, 0), Vector3.back);
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(topTriangles.Concat(sideTriangles).ToList(), 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;

            void AddWall(Vector2 left, Vector2 right, Vector3 normal)
            {
                int start = vertices.Count;

                vertices.Add(new Vector3(left.x - 0.5f, -0.5f, left.y - 0.5f));
                vertices.Add(new Vector3(right.x - 0.5f, -0.5f, right.y - 0.5f));
                vertices.Add(new Vector3(right.x - 0.5f, 0.5f + TileShape.Height(right.x, right.y, bitMaskForPathsConnections), right.y - 0.5f));
                vertices.Add(new Vector3(left.x - 0.5f, 0.5f + TileShape.Height(left.x, left.y, bitMaskForPathsConnections), left.y - 0.5f));

                for (int n = 0; n < 4; n++)
                {
                    normals.Add(normal);
                    uvs.Add(sideUV);
                }

                sideTriangles.AddRange(new[] { start, start + 3, start + 2, start, start + 2, start + 1 });
            }
        }

        // Even grid plus extra lines where the bevel bends
        private static List<float> GridCoords(int resolution)
        {
            SortedSet<float> coords = new() { 0.0175f, 0.035f, 0.9650f, 0.9825f };

            for (int i = 0; i <= resolution; i++)
                coords.Add((float)i / resolution);

            return new List<float>(coords);
        }
    }

}
