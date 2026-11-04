using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Units;


namespace TowerDefense.Main.Managers.WaveSpawners
{
    public abstract class WaveSpawner: MonoBehaviour
    {
        public Transform SpawnPoint { get; protected set; }

        protected UnitsStorage unitsStorage;

        [SerializeField]
        protected float timeBetweenWaves = 5f;
        public float timeToNextSpawn { get; protected set; } = 5f;
        public float TimeBetweenWaves { get { return timeBetweenWaves; } }

        public int WaveNumber { get; protected set; } = 1;

        protected List<UnitType> availableUnit;
        protected Dictionary<UnitType, int> availablePrices;

        protected List<UnitType> nextWave;
        
        protected int countAllUnitsCurrentWave;
        protected int countSpawnedUnitsInThisWave;
        protected bool AllCurrentWaveUnitsSpawned;

        public virtual void Initialize(Transform spawn)
        {
            SpawnPoint = spawn;
            unitsStorage = Resources.Load<UnitsStorage>($"{nameof(UnitsStorage)}");

            countAllUnitsCurrentWave = 0;
            countSpawnedUnitsInThisWave = 0;
            AllCurrentWaveUnitsSpawned = false;
        }


        public bool WaveFinished()
        {
            return countSpawnedUnitsInThisWave == countAllUnitsCurrentWave && GameObject.FindGameObjectsWithTag(Unit.unitTag).Length == 0;
        }

        protected void SpawnUnit(UnitType unitType)
        {
            countSpawnedUnitsInThisWave++;
            Instantiate(unitsStorage.Units[(int)unitType], SpawnPoint.position, SpawnPoint.rotation);
        }

        protected void UpdateAvailableUnit()
        {
            switch (WaveNumber)
            {
                case 1:
                    availableUnit.Add(UnitType.Goblin);
                    availablePrices[UnitType.Goblin] = unitsStorage.Units[(int)UnitType.Goblin].GetComponent<Unit>().Price;
                    break;
                case 4:
                    availablePrices[UnitType.Bat] = unitsStorage.Units[(int)UnitType.Bat].GetComponent<Unit>().Price;
                    availableUnit.Add(UnitType.Bat);
                    break;
                case 6:
                    availablePrices[UnitType.OrcWargRider] = unitsStorage.Units[(int)UnitType.OrcWargRider].GetComponent<Unit>().Price;
                    availableUnit.Add(UnitType.OrcWargRider);
                    break;
                case 8:
                    availablePrices[UnitType.Troll] = unitsStorage.Units[(int)UnitType.Troll].GetComponent<Unit>().Price;
                    availableUnit.Add(UnitType.Troll);
                    break;

                default:
                    break;
            }
        }

        protected void OperationPrevStartWave()
        {
            countSpawnedUnitsInThisWave = 0;
            countAllUnitsCurrentWave = nextWave.Count;
        }

        protected void OperationAfterFinishWave()
        {
            WaveNumber++;
            PlayerStats.IncreaseMoneyAfterWave();

            UpdateMoneyByWave();
            UpdateGameStats();
        }

        protected void UpdateMoneyByWave()
        {
            int numberNextWave = WaveNumber + 1;
            if (numberNextWave % 5 == 0)
            {
                PlayerStats.IncreaseMoneyByWave(2);
            }
        }

        protected void UpdateGameStats()
        {
            if (WaveNumber % 5 == 0)
            {
                foreach (var unit in unitsStorage.Units)
                {
                    unit.GetComponent<Unit>().UpgradeHealth((float)0.25);
                }
            }
        }
    }

}
