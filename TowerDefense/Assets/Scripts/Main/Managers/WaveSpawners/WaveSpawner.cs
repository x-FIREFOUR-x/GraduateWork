using System;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Units;
using TowerDefense.Main.Units.Squad;


namespace TowerDefense.Main.Managers.WaveSpawners
{
    public abstract class WaveSpawner: MonoBehaviour
    {
        protected UnitsStorage unitsStorage;

        [SerializeField]
        protected UnlockScheduleController unlockScheduleController;

        public Transform SpawnPoint { get; protected set; }
        [field: SerializeField]
        public float UnitSpawnInterval { get; private set; } = 0.5f;

        [SerializeField]
        protected float timeBetweenWaves = 5f;
        public float timeToNextSpawn { get; protected set; } = 5f;
        public float TimeBetweenWaves { get { return timeBetweenWaves; } }

        [SerializeField]
        private float healthUpgradeMultiplier = 1.25f;
        public float HealthMultiplier { get; private set; } = 1f;

        public int WaveNumber { get; protected set; } = 1;

        protected List<UnitType> nextWave;
        public IReadOnlyList<UnitType> LastWave => (IReadOnlyList<UnitType>)nextWave ?? Array.Empty<UnitType>();
        
        protected int countAllUnitsCurrentWave;
        protected int countSpawnedUnitsInThisWave;
        protected bool AllCurrentWaveUnitsSpawned;

        private readonly List<SquadLeader> squadLeadersAbleToRecruit = new();

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
            GameObject unitObject = Instantiate(unitsStorage.Units[(int)unitType], SpawnPoint.position, SpawnPoint.rotation);
            Unit unit = unitObject.GetComponent<Unit>();
            unit.ApplyHealthMultiplier(HealthMultiplier);

            RecruitToSquad(unit);

            if (unit is SquadLeader squadLeader)
                squadLeadersAbleToRecruit.Add(squadLeader);
        }

        private void RecruitToSquad(Unit unit)
        {
            squadLeadersAbleToRecruit.RemoveAll(squadLeader => !squadLeader.CanRecruit);

            foreach (SquadLeader squadLeader in squadLeadersAbleToRecruit)
            {
                if (squadLeader.TryRecruit(unit))
                    return;
            }
        }

        protected void OperationPrevStartWave()
        {
            countSpawnedUnitsInThisWave = 0;
            countAllUnitsCurrentWave = nextWave.Count;
            squadLeadersAbleToRecruit.Clear();
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
            if (numberNextWave % 3 == 0)
            {
                PlayerStats.IncreaseMoneyByWave(1.5f);
            }
        }

        protected void UpdateGameStats()
        {
            if (WaveNumber % 5 == 0)
                HealthMultiplier *= healthUpgradeMultiplier;
        }
    }

}
