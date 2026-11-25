using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Serialization;

using TowerDefense.Main.Map.Buildings;
using TowerDefense.Main.Map.Path;
using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.State;
using TowerDefense.Opponents.Attacker.Strategies;
using TowerDefense.Opponents.SimulationBridge;


namespace TowerDefense.Main.Managers.WaveSpawners
{
    public class AIWaveSpawner : WaveSpawner
    {
        public const int FirstWaveMaxUnits = 3;
        public const string SimulationInputFileName = "simulation-input-attacker.json";

        public enum AttackerDecisionAlgorithm
        {
            OriginalGa = 0,
            NsgaII = 1,
            LegacyGa = 2
        }

        [SerializeField, FormerlySerializedAs("strategy")]
        private AttackerDecisionAlgorithm decisionAlgorithm = AttackerDecisionAlgorithm.OriginalGa;
        [SerializeField, Range(0.1f, 1f)]
        private float decisionTimeShare = 0.9f;
        [SerializeField]
        private bool useSeed = false;
        [SerializeField]
        private int seed = 0;

        [Header("NSGA-II")]
        [SerializeField, Range(0f, 1f), FormerlySerializedAs("beta")]
        private float difficultyLevel = 0.7f;
        [SerializeField, Range(0f, 1f)]
        private float minDifficultyLevel = 0.5f;
        [SerializeField]
        private bool adaptiveDifficulty = true;
        [SerializeField]
        private float targetCastleDamage = 10f;
        [SerializeField]
        private float regulatorStep = 0.1f;

        private IAttackDecisionStrategy decisionStrategy;
        private AttackerBudget attackerBudget;
        private TilesMap tilesMap;
        private AttackerSnapshot decisionSnapshot;
        private Task<List<UnitType>> decisionTask;
        private int castleHealthBeforeWave;
        private bool isSpawning;


        public override void Initialize(Transform spawn)
        {
            base.Initialize(spawn);

            attackerBudget = new AttackerBudget();
            tilesMap = FindAnyObjectByType<TilesMap>();
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


        private IAttackDecisionStrategy CreateStrategy()
        {
            TimeSpan timeLimit = TimeSpan.FromSeconds(TimeBetweenWaves * decisionTimeShare);
            int? strategySeed = useSeed ? seed : (int?)null;

            switch (decisionAlgorithm)
            {
                case AttackerDecisionAlgorithm.NsgaII:
                    return new NsgaIIAttackStrategy(timeLimit, strategySeed, difficultyLevel, minDifficultyLevel, adaptiveDifficulty, targetCastleDamage, regulatorStep);
                case AttackerDecisionAlgorithm.LegacyGa:
                    return new LegacyGaAttackStrategy(timeLimit, strategySeed);
                default:
                    return new OriginalGaAttackStrategy(timeLimit, strategySeed);
            }
        }

        private void GenerateWave()
        {
            UpdateAvailableUnit();

            AttackerSnapshot snapshot = new AttackerSnapshot(availableUnit, availablePrices, CollectBuiltTowers(), Mathf.Max(0, attackerBudget.MoneyForWave()), CreateSimulationRequest());
            decisionSnapshot = snapshot;
            decisionTask = Task.Run(() => decisionStrategy.Decide(snapshot));
        }

        private List<UnitType> TakeDecision()
        {
            if (decisionTask.IsFaulted)
            {
                Debug.LogException(decisionTask.Exception);
                return GreedyAttack.Build(decisionSnapshot);
            }

            Debug.Log($"Wave {WaveNumber}: {decisionStrategy.LastReport}");
            return decisionTask.Result;
        }

        private IEnumerator SpawnWave(List<UnitType> wave)
        {
            nextWave = wave.Take(MaxUnitsInWave(WaveNumber)).ToList();
            castleHealthBeforeWave = CastleHealth();

            OperationPrevStartWave();

            for (int i = 0; i < nextWave.Count; i++)
            {
                SpawnUnit(nextWave[i]);
                yield return new WaitForSeconds(UnitSpawnInterval);
            }

            AllCurrentWaveUnitsSpawned = true;
            isSpawning = false;
        }

        public static int MaxUnitsInWave(int waveNumber) => waveNumber == 1 ? FirstWaveMaxUnits : int.MaxValue;

        private SimulationRequest CreateSimulationRequest()
        {
            string inputPath = SimulationInputDataWriter.Write(tilesMap, PathGraphHolder.Graph, SpawnPoint.position, SimulationInputFileName);
            return new SimulationRequest(inputPath, SimulationTowerPlacementConverter.CollectBuiltTowers(tilesMap), MaxUnitsInWave(WaveNumber), WaveNumber);
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
