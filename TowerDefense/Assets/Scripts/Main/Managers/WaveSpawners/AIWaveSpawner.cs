using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Serialization;

using TowerDefense.Main.Map.Buildings;
using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.State;
using TowerDefense.Opponents.Attacker.Strategies;


namespace TowerDefense.Main.Managers.WaveSpawners
{
    public class AIWaveSpawner : WaveSpawner
    {
        public enum AttackerDecisionAlgorithm
        {
            OriginalGa = 0,
            NsgaII = 1,
            LegacyGa = 2
        }

        [SerializeField, FormerlySerializedAs("strategy")]
        private AttackerDecisionAlgorithm decisionAlgorithm = AttackerDecisionAlgorithm.OriginalGa;
        [SerializeField]
        private float timeLimitSeconds = 2f;
        [SerializeField]
        private bool useSeed = false;
        [SerializeField]
        private int seed = 0;

        [Header("NSGA-II")]
        [SerializeField, Range(0f, 1f), FormerlySerializedAs("beta")]
        private float difficultyLevel = 0.7f;
        [SerializeField]
        private bool adaptiveDifficulty = true;
        [SerializeField]
        private float targetCastleDamage = 10f;
        [SerializeField]
        private float regulatorStep = 0.02f;

        private IWaveDecisionStrategy decisionStrategy;
        private AttackerBudget attackerBudget;
        private AttackerSnapshot decisionSnapshot;
        private Task<List<UnitType>> decisionTask;
        private int castleHealthBeforeWave;
        private bool isSpawning;


        public override void Initialize(Transform spawn)
        {
            base.Initialize(spawn);

            attackerBudget = new AttackerBudget();
            decisionStrategy = CreateStrategy();

            availableUnit = new List<UnitType>();
            availablePrices = new Dictionary<UnitType, int>();
        }


        void Update()
        {
            if (isSpawning)
                return;

            if (!WaveFinished())
                return;

            if (AllCurrentWaveUnitsSpawned)
            {
                OperationAfterFinishWave();
                decisionStrategy.OnWaveFinished(Mathf.Max(0, castleHealthBeforeWave - CastleHealth()));
                AllCurrentWaveUnitsSpawned = false;
            }

            timeToNextSpawn = Mathf.Max(0f, timeToNextSpawn - Time.deltaTime);

            if (decisionTask == null)
            {
                GenerateWave();
                return;
            }

            if (timeToNextSpawn <= 0f && decisionTask.IsCompleted)
            {
                List<UnitType> wave = TakeDecision();
                decisionTask = null;
                isSpawning = true;
                StartCoroutine(SpawnWave(wave));
                timeToNextSpawn = timeBetweenWaves;
            }
        }


        private IWaveDecisionStrategy CreateStrategy()
        {
            TimeSpan timeLimit = TimeSpan.FromSeconds(timeLimitSeconds);
            int? strategySeed = useSeed ? seed : (int?)null;

            switch (decisionAlgorithm)
            {
                case AttackerDecisionAlgorithm.NsgaII:
                    return new NsgaIIWaveStrategy(timeLimit, strategySeed, difficultyLevel, adaptiveDifficulty, targetCastleDamage, regulatorStep);
                case AttackerDecisionAlgorithm.LegacyGa:
                    return new LegacyGaWaveStrategy(timeLimit, strategySeed);
                default:
                    return new OriginalGaWaveStrategy(timeLimit, strategySeed);
            }
        }

        private void GenerateWave()
        {
            UpdateAvailableUnit();

            AttackerSnapshot snapshot = new AttackerSnapshot(availableUnit, availablePrices, CollectBuiltTowers(), Mathf.Max(0, attackerBudget.MoneyForWave()));
            decisionSnapshot = snapshot;
            decisionTask = Task.Run(() => decisionStrategy.Decide(snapshot));
        }

        private List<UnitType> TakeDecision()
        {
            if (decisionTask.IsFaulted)
            {
                Debug.LogException(decisionTask.Exception);
                return GreedyWave.Build(decisionSnapshot);
            }

            Debug.Log($"Wave {WaveNumber}: {decisionStrategy.LastReport}");
            return decisionTask.Result;
        }

        private IEnumerator SpawnWave(List<UnitType> wave)
        {
            nextWave = WaveNumber == 1 ? wave.Take(3).ToList() : wave;
            castleHealthBeforeWave = CastleHealth();

            OperationPrevStartWave();

            for (int i = 0; i < nextWave.Count; i++)
            {
                SpawnUnit(nextWave[i]);
                yield return new WaitForSeconds(0.5f);
            }

            AllCurrentWaveUnitsSpawned = true;
            isSpawning = false;
        }

        private static List<TowerType> CollectBuiltTowers()
        {
            return GameObject.FindGameObjectsWithTag(Tower.towerTag)
                .Select(tower => tower.GetComponent<Tower>().Type)
                .ToList();
        }

        private static int CastleHealth()
        {
            GameObject castle = GameObject.FindGameObjectWithTag(EndBuilding.endBuildingTag);
            return castle == null ? 0 : castle.GetComponent<EndBuilding>().Health;
        }
    }
}
