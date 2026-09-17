using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Main.Map.Tiles
{
    // Which sides of a path tile continue into a neighbor path tile, stored as bit flags (North | East | South | West)
    // It defines the path shape: straight, corner, end, junction. World directions: north = +Z, east = +X.
    public static class TilePathConnections
    {
        public const int North = 1;
        public const int East = 2;
        public const int South = 4;
        public const int West = 8;
        public const int CombinationCount = 16;


        public static int GetBitMaskFromConnectedSides(bool isNorthConnection, bool isEastConnection, bool isSouthConnection, bool isWestConnection)
        {
            return (isNorthConnection ? North : 0) | (isEastConnection ? East : 0) | (isSouthConnection ? South : 0) | (isWestConnection ? West : 0);
        }

        public static int GetBitMaskFromPathNeighbors(List<Vector2Int> path, int indexInPath)
        {
            bool isNorthConnection = false;
            bool isSouthConnection = false;
            bool isEastConnection = false;
            bool isWestConnection = false;

            foreach (int neighborIndex in new[] { indexInPath - 1, indexInPath + 1 })
            {
                if (neighborIndex < 0 || neighborIndex >= path.Count)
                    continue;

                Vector2Int delta = path[neighborIndex] - path[indexInPath];
                isNorthConnection |= delta == new Vector2Int(1, 0);
                isSouthConnection |= delta == new Vector2Int(-1, 0);
                isEastConnection |= delta == new Vector2Int(0, 1);
                isWestConnection |= delta == new Vector2Int(0, -1);
            }

            return GetBitMaskFromConnectedSides(isNorthConnection, isEastConnection, isSouthConnection, isWestConnection);
        }
    }

}
