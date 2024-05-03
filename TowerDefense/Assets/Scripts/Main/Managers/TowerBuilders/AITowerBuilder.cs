using System;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Collections;
using TowerDefense.Main.Map;
using TowerDefense.Main.Map.Tiles;
using TowerDefense.Main.Managers.WaveSpawners;
using TowerDefense.Main.Towers;
using TowerDefense.Algorithms.TowerSelectionAlgorithm;


namespace TowerDefense.Main.Managers.TowerBuilders
{
    public class AITowerBuilder : MonoBehaviour
    {
        [SerializeField]
        private MapComponentsController mapComponentController;
        private TilesMap tilesMap;

        [SerializeField]
        private WaveSpawner waveSpawner;

        [Header("Effect")]
        [SerializeField]
        private GameObject effectBuildingPrefab;
        [SerializeField]
        private float timeEffectBuilding;


        private TowersStorage towersStorage;

        private TowerSelectionAlgorithm towerSelectionAlgorithm;
        private List<TowerType> availableTowersTypes;
        private Dictionary<TowerType, int> availableTowersPrices;

        private int countBuildedTowerList = 0;


        private void Start()
        {
            towersStorage = Resources.Load<TowersStorage>($"{nameof(TowersStorage)}");

            tilesMap = mapComponentController.TilesMap;
            InitializeTowerSelectionAlgorithm();
        }

        private void Update()
        {
            if(!waveSpawner.WaveNotFinished() && countBuildedTowerList != waveSpawner.WaveNumber)
            {
                UpdateAvailableTower();

                List<Tuple<TowerType, Vector2Int>> towerList = towerSelectionAlgorithm.GenerateTowerList(availableTowersTypes, availableTowersPrices, PlayerStats.MoneyDefender);
                foreach (var towerAndTile in towerList)
                {
                    TowerTile towerTile = tilesMap.GetTileAt(towerAndTile.Item2.x, towerAndTile.Item2.y).GetComponent<TowerTile>();
                    BuildTower(towerAndTile.Item1, towerTile);
                }

                countBuildedTowerList++;
            }
        }


        private void InitializeTowerSelectionAlgorithm()
        {
            Dictionary<TowerType, float> sharesTowerType = new Dictionary<TowerType, float> {
                [TowerType.Turret] = 0.15f,
                [TowerType.PanelsTurret] = 0.3f,
                [TowerType.RocketLauncher] = 0.3f,
                [TowerType.LaserTurret] = 0.25f,
            };

            availableTowersTypes = new List<TowerType>();
            availableTowersPrices = new Dictionary<TowerType, int>();

            towerSelectionAlgorithm = new TowerSelectionAlgorithm(CreatePriorityQueueTilesForTowers(), sharesTowerType);
        }

        private void UpdateAvailableTower()
        {
            
            switch (countBuildedTowerList)
            {
                case 0:
                    availableTowersTypes.Add(TowerType.Turret);
                    availableTowersPrices[TowerType.Turret] = towersStorage.Towers[(int)TowerType.Turret].GetComponent<Tower>().Price;
                    break;
                case 2:
                    availableTowersTypes.Add(TowerType.PanelsTurret);
                    availableTowersPrices[TowerType.PanelsTurret] = towersStorage.Towers[(int)TowerType.PanelsTurret].GetComponent<Tower>().Price;
                    break;
                case 4:
                    availableTowersTypes.Add(TowerType.RocketLauncher);
                    availableTowersPrices[TowerType.RocketLauncher] = towersStorage.Towers[(int)TowerType.RocketLauncher].GetComponent<Tower>().Price;
                    break;
                case 6:
                    availableTowersTypes.Add(TowerType.LaserTurret);
                    availableTowersPrices[TowerType.LaserTurret] = towersStorage.Towers[(int)TowerType.LaserTurret].GetComponent<Tower>().Price;
                    break;

                default:
                    break;
            }
        }


        private void BuildTower(TowerType towerType, TowerTile tile)
        {
            Tower tower = towersStorage.Towers[(int)towerType].GetComponent<Tower>();
            GameObject towerObj = Instantiate(tower.gameObject, tile.GetTowerBuildPosition(), tile.transform.rotation);
            tile.Tower = towerObj;

            GameObject effect = Instantiate(effectBuildingPrefab, tile.GetTowerBuildPosition(), tile.transform.rotation);
            Destroy(effect, timeEffectBuilding);

            PlayerStats.AddDefenderMoney(-tower.Price);
        }

        private PriorityQueueWithDictOfPriorities<Vector2Int, TowerType, int> CreatePriorityQueueTilesForTowers()
        {
            PriorityQueueWithDictOfPriorities<Vector2Int, TowerType, int> priorityQueueTowerTiles = new((a, b) => a > b);
            for (int i = 0; i < tilesMap.Size; i++)
            {
                for (int j = 0; j < tilesMap.Size; j++)
                {
                    TowerTile towerTile = tilesMap.GetTileAt(i, j).GetComponent<TowerTile>();

                    if (towerTile == null)
                        continue;

                    Dictionary<TowerType, int> dictOfPriorities = CreateDictOfPrioritiesForTowerTile(towerTile.GetTowerBuildPosition());
                    priorityQueueTowerTiles.Add(new Vector2Int(i, j), dictOfPriorities);
                }
            }

            return priorityQueueTowerTiles;
        }

        private Dictionary<TowerType, int> CreateDictOfPrioritiesForTowerTile(Vector3 towerTilePosition)
        {
            Dictionary<TowerType, int> dictOfPriorities = new();
            foreach (var towerPrefab in towersStorage.Towers)
            {
                Tower tower = towerPrefab.GetComponent<Tower>();
                int countPathTilesInShootRangeTower = 0;

                Collider[] colliderInShootRange = Physics.OverlapSphere(towerTilePosition, tower.ShootRange);
                foreach (Collider collider in colliderInShootRange)
                {
                    if (collider.tag == PathTile.pathTileTag)
                    {
                        countPathTilesInShootRangeTower++;
                    }
                }

                dictOfPriorities[tower.Type] = countPathTilesInShootRangeTower;
            }

            return dictOfPriorities;
        }
    }

}
