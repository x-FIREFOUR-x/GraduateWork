using UnityEngine;

using TowerDefense.Main.Map.Tile;


namespace TowerDefense.EditorTools.Tile
{
    // Relief of a tile top in tile space: u grows along +X (east), v along +Z (north), both in [0, 1].
    // Path connections are a TilePathConnections value, grass tiles use NoPath.
    public static class TileShape
    {
        public const int NoPath = -1;
        private const int North = TilePathConnections.North;
        private const int East = TilePathConnections.East;
        private const int South = TilePathConnections.South;
        private const int West = TilePathConnections.West;

        // Must match PATH_W in the texture generator
        private const float pathHalfWidth = 0.36f;
        private const float pathEdgeBand = 0.03f;
        private const float pathDepth = 0.1f;

        private const float bevelWidth = 0.035f;
        private const float bevelDrop = 0.08f;


        // Height below the flat top in world units (tile Y scale is 1)
        public static float Height(float u, float v, int bitMaskForPathsConnections)
        {
            float edge = Mathf.Min(Mathf.Min(u, 1 - u), Mathf.Min(v, 1 - v));
            float bevel = bevelDrop * (1 - Mathf.SmoothStep(0, 1, edge / bevelWidth));

            if (bitMaskForPathsConnections == NoPath)
                return -bevel;

            float dirt = Dirtness(u, v, bitMaskForPathsConnections);
            return Mathf.Lerp(-bevel, -pathDepth, dirt);
        }

        // 1 on the path, 0 on grass
        public static float Dirtness(float u, float v, int bitMaskForPathsConnections)
        {
            if (bitMaskForPathsConnections == NoPath)
                return 0;

            float t = Mathf.Clamp01((PathDistance(u, v, bitMaskForPathsConnections) + pathEdgeBand) / (2 * pathEdgeBand));
            return 1 - t * t * (3 - 2 * t);
        }

        // Signed distance to the path border, negative inside the path
        public static float PathDistance(float u, float v, int bitMaskForPathsConnections)
        {
            float x = u - 0.5f;
            float z = v - 0.5f;

            if (bitMaskForPathsConnections == 0)
                return BoxDistance(x, z, pathHalfWidth - pathHalfWidth * 0.45f, pathHalfWidth - pathHalfWidth * 0.45f) - pathHalfWidth * 0.45f;

            float distance = Mathf.Sqrt(x * x + z * z) - pathHalfWidth;

            if ((bitMaskForPathsConnections & North) != 0) distance = Mathf.Min(distance, BoxDistance(x, z - 1, pathHalfWidth, 1));
            if ((bitMaskForPathsConnections & East) != 0) distance = Mathf.Min(distance, BoxDistance(x - 1, z, 1, pathHalfWidth));
            if ((bitMaskForPathsConnections & South) != 0) distance = Mathf.Min(distance, BoxDistance(x, z + 1, pathHalfWidth, 1));
            if ((bitMaskForPathsConnections & West) != 0) distance = Mathf.Min(distance, BoxDistance(x + 1, z, 1, pathHalfWidth));

            return distance;
        }

        private static float BoxDistance(float x, float z, float extentX, float extentZ)
        {
            float qx = Mathf.Abs(x) - extentX;
            float qz = Mathf.Abs(z) - extentZ;

            float outside = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qz, 0)).magnitude;
            float inside = Mathf.Min(Mathf.Max(qx, qz), 0);

            return outside + inside;
        }
    }

}
