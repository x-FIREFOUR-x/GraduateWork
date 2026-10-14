using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Main.Map.Tile
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

        public static int GetBitMaskFromPathCells(HashSet<Vector2Int> pathCells, Vector2Int cell)
        {
            return GetBitMaskFromConnectedSides(pathCells.Contains(cell + new Vector2Int(1, 0)),
                                                pathCells.Contains(cell + new Vector2Int(0, 1)),
                                                pathCells.Contains(cell + new Vector2Int(-1, 0)),
                                                pathCells.Contains(cell + new Vector2Int(0, -1)));
        }
    }

}
