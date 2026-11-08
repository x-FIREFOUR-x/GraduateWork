using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Units;
using TowerDefense.Main.UI.UnitMenu;


namespace TowerDefense.Main.Managers.WaveSpawners
{
    public class PlayerWaveSpawner : WaveSpawner
    {
        [SerializeField]
        private UnitShopMenu unitShopMenu;

        public override void Initialize(Transform spawn)
        {
            base.Initialize(spawn);

            availableUnit = new List<UnitType>();
            availablePrices = new Dictionary<UnitType, int>();
        }

        void Update()
        {
            if (!WaveFinished())
            {
                return;
            }
            else
            {
                if (AllCurrentWaveUnitsSpawned)
                {
                    OperationAfterFinishWave();
                    AllCurrentWaveUnitsSpawned = false;
                }
            }

            if (timeToNextSpawn <= 0f)
            {
                StartCoroutine(SpawnWave());
                timeToNextSpawn = timeBetweenWaves;
            }
            timeToNextSpawn -= Time.deltaTime;
            timeToNextSpawn = Mathf.Clamp(timeToNextSpawn, 0f, Mathf.Infinity);
        }

        private IEnumerator SpawnWave()
        {
            nextWave = unitShopMenu.TakeUnits();
            OperationPrevStartWave();

            for (int i = 0; i < nextWave.Count; i++)
            {
                SpawnUnit(nextWave[i]);
                yield return new WaitForSeconds(0.5f);
            }

            AllCurrentWaveUnitsSpawned = true;
        }
    }

}
