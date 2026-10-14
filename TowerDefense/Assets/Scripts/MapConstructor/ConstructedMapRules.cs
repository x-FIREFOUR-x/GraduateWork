using System;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.MapConstructor
{
    public static class ConstructedMapRules
    {
        private static readonly Vector2Int[] Directions = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };
        private static readonly Vector2Int[] Diagonals = { new(1, 1), new(1, -1), new(-1, 1), new(-1, -1) };


        public static MapValidationError CheckPathLayout(Func<Vector2Int, bool> isPathCell, Vector2Int? startCell, Vector2Int? endCell,
                                                    IEnumerable<Vector2Int> cellsToCheckForSquare)
        {
            if (startCell.HasValue && CountPathNeighbors(startCell.Value, isPathCell) > 1)
                return MapValidationError.StartBuildingHasSeveralConnections;

            if (endCell.HasValue && CountPathNeighbors(endCell.Value, isPathCell) > 1)
                return MapValidationError.EndBuildingHasSeveralConnections;

            foreach (Vector2Int cell in cellsToCheckForSquare)
            {
                if (isPathCell(cell) && IsInPathSquare(cell, isPathCell))
                    return MapValidationError.PathSquare;
            }

            return MapValidationError.None;
        }

        public static MapValidationError CheckBuildingsExist(bool hasStartBuilding, bool hasEndBuilding)
        {
            if (!hasStartBuilding && !hasEndBuilding)
                return MapValidationError.MissingStartAndEndBuildings;

            if (!hasStartBuilding)
                return MapValidationError.MissingStartBuilding;

            return hasEndBuilding ? MapValidationError.None : MapValidationError.MissingEndBuilding;
        }

        public static MapValidationError ConvertToMapValidationError(PathGraphBuildError error)
        {
            return error switch
            {
                PathGraphBuildError.StartIsEnd => MapValidationError.StartIsEnd,
                PathGraphBuildError.NoRoute => MapValidationError.NoRoute,
                PathGraphBuildError.TooManyRoutes => MapValidationError.TooManyRoutes,
                PathGraphBuildError.CellsOutsideRoutes => MapValidationError.CellsOutsideRoutes,
                PathGraphBuildError.Cycle => MapValidationError.Cycle,
                _ => MapValidationError.None
            };
        }

        private static int CountPathNeighbors(Vector2Int cell, Func<Vector2Int, bool> isPathCell)
        {
            int countNeighbors = 0;

            foreach (Vector2Int direction in Directions)
            {
                if (isPathCell(cell + direction))
                    countNeighbors++;
            }

            return countNeighbors;
        }

        private static bool IsInPathSquare(Vector2Int cell, Func<Vector2Int, bool> isPathCell)
        {
            foreach (Vector2Int diagonal in Diagonals)
            {
                if (isPathCell(cell + new Vector2Int(diagonal.x, 0)) && isPathCell(cell + new Vector2Int(0, diagonal.y)) && isPathCell(cell + diagonal))
                    return true;
            }

            return false;
        }
    }

}
