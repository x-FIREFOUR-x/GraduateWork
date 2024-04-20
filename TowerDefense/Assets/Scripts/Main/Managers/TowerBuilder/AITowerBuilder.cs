using System;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Collections;

namespace TowerDefense.TowerBuilder
{
    public class AITowerBuilder : MonoBehaviour
    {
        [SerializeField]
        private MapComponentsController mapComponentController;
        private TilesMap tilesMap;

        [SerializeField]
        private WaveSpawner waveSpawner;


        private TowersStorage towersStorage;

        //private TowerGeneratorAlgorithm;
        PriorityQueueWithDictOfPriorities<TowerTile, TowerType, int> pQ;
        private int countBuildedTowerList = 0;

        private void Start()
        {
            towersStorage = Resources.Load<TowersStorage>($"{nameof(TowersStorage)}");

            tilesMap = mapComponentController.TilesMap;
            //TowerGeneratorAlgorithm(CreatePriorityQueueTilesForTowers());
            pQ = CreatePriorityQueueTilesForTowers();
        }

        private void Update()
        {
            if(!waveSpawner.WaveNotFinished() && countBuildedTowerList != waveSpawner.WaveNumber)
            {
                //List<Tuple<TowerType, TowerTile>> towerList = TowerGeneratorAlgorithm.GenerateTowerList(PlayerStats.MoneyDefender);
                List<Tuple<TowerType, TowerTile>> towerAndTileList = new List<Tuple<TowerType, TowerTile>> { new Tuple<TowerType, TowerTile>(TowerType.Turret, pQ.Peek(TowerType.Turret))};

                foreach (var towerAndTile in towerAndTileList)
                {
                    BuildTower(towerAndTile.Item1, towerAndTile.Item2);
                }

                countBuildedTowerList++;
            }
        }

        private void BuildTower(TowerType towerType, TowerTile tile)
        {
            Tower tower = towersStorage.Towers[(int)towerType].GetComponent<Tower>();
            GameObject towerObj = Instantiate(tower.gameObject, tile.GetTowerBuildPosition(), tile.transform.rotation);
            tile.Tower = towerObj;

            PlayerStats.AddDefenderMoney(-tower.Price);
        }

        private PriorityQueueWithDictOfPriorities<TowerTile, TowerType, int> CreatePriorityQueueTilesForTowers()
        {
            PriorityQueueWithDictOfPriorities<TowerTile, TowerType, int> priorityQueueTowerTiles = new((a, b) => a > b);
            for (int i = 0; i < tilesMap.Size; i++)
            {
                for (int j = 0; j < tilesMap.Size; j++)
                {
                    TowerTile towerTile = tilesMap.GetTileAt(i, j).GetComponent<TowerTile>();

                    if (towerTile == null)
                        continue;

                    Dictionary<TowerType, int> dictOfPriorities = CreateDictOfPrioritiesForTowerTile(towerTile.GetTowerBuildPosition());
                    priorityQueueTowerTiles.Add(towerTile, dictOfPriorities);
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
