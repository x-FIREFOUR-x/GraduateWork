using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Towers;


namespace TowerDefense.Opponents.Defender.State
{
    public sealed class PathCoverageMap
    {
        private readonly Dictionary<DefenderGene, int[]> _pathCellsInRangeByTowerPlacement;

        public int PathCellCount { get; }


        public PathCoverageMap(int pathCellCount, IReadOnlyDictionary<DefenderGene, int[]> pathCellsInRangeByTowerPlacement)
        {
            if (pathCellCount < 0)
                throw new ArgumentOutOfRangeException(nameof(pathCellCount), pathCellCount, "Path cell count must be non-negative.");
            if (pathCellsInRangeByTowerPlacement == null)
                throw new ArgumentNullException(nameof(pathCellsInRangeByTowerPlacement));

            PathCellCount = pathCellCount;
            _pathCellsInRangeByTowerPlacement = new Dictionary<DefenderGene, int[]>();
            foreach (var placement in pathCellsInRangeByTowerPlacement)
            {
                if (placement.Value.Any(index => index < 0 || index >= pathCellCount))
                    throw new ArgumentOutOfRangeException(nameof(pathCellsInRangeByTowerPlacement), $"Covered path cells of {placement.Key} must be in [0; {pathCellCount}).");
                _pathCellsInRangeByTowerPlacement[placement.Key] = placement.Value.Distinct().ToArray();
            }
        }

        public IReadOnlyList<int> CoveredPathCells(DefenderGene placement) =>
            _pathCellsInRangeByTowerPlacement.TryGetValue(placement, out int[] coveredPathCells) ? coveredPathCells : Array.Empty<int>();

        public static PathCoverageMap Build(TilesMap tilesMap, IEnumerable<Tower> towers)
        {
            if (tilesMap == null)
                throw new ArgumentNullException(nameof(tilesMap));
            if (towers == null)
                throw new ArgumentNullException(nameof(towers));

            List<Tower> towerList = towers.ToList();
            GameObject[] pathTiles = GameObject.FindGameObjectsWithTag(PathTile.pathTileTag);
            var pathCellIndexByTile = new Dictionary<GameObject, int>();
            for (int i = 0; i < pathTiles.Length; i++)
                pathCellIndexByTile[pathTiles[i]] = i;

            var pathCellsInRangeByTowerPlacement = new Dictionary<DefenderGene, int[]>();
            for (int row = 0; row < tilesMap.Size; row++)
            {
                for (int column = 0; column < tilesMap.Size; column++)
                {
                    TowerTile towerTile = tilesMap.GetTowerTileAt(row, column);
                    if (towerTile == null)
                        continue;

                    Vector3 buildPosition = towerTile.GetTowerBuildPosition();
                    foreach (Tower tower in towerList)
                    {
                        var coveredPathCells = new HashSet<int>();
                        foreach (Collider collider in Physics.OverlapSphere(buildPosition, tower.ShootRange))
                            if (pathCellIndexByTile.TryGetValue(collider.gameObject, out int pathCellIndex))
                                coveredPathCells.Add(pathCellIndex);

                        pathCellsInRangeByTowerPlacement[new DefenderGene(tower.Type, new Vector2Int(row, column))] = coveredPathCells.ToArray();
                    }
                }
            }

            return new PathCoverageMap(pathTiles.Length, pathCellsInRangeByTowerPlacement);
        }
    }
}
