using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Towers;
using TowerDefense.Opponents.Defender.State;
using Simulation;
using Simulation.Geometry;


namespace TowerDefense.Opponents.SimulationBridge
{
    public static class SimulationTowerPlacementConverter
    {
        public static PlacedTower Of(TowerType type, Vector2Int cell) => new PlacedTower(type.ToString(), new Cell(cell.x, cell.y));

        public static PlacedTower Of(DefenderGene tower) => Of(tower.Type, tower.Cell);

        public static List<PlacedTower> CollectBuiltTowers(TilesMap tilesMap)
        {
            var towers = new List<PlacedTower>();
            for (int row = 0; row < tilesMap.Size; row++)
            {
                for (int column = 0; column < tilesMap.Size; column++)
                {
                    TowerTile towerTile = tilesMap.GetTowerTileAt(row, column);
                    if (towerTile != null && towerTile.Tower != null)
                        towers.Add(Of(towerTile.Tower.GetComponent<Tower>().Type, new Vector2Int(row, column)));
                }
            }
            return towers;
        }
    }
}
