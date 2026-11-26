using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Map;
using TowerDefense.Main.Map.Buildings;
using TowerDefense.Main.Map.Path;
using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Managers.WaveSpawners;
using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Opponents.Defender.State;
using TowerDefense.Opponents.Defender.Strategies;
using TowerDefense.Opponents.SimulationBridge;


namespace TowerDefense.Main.Managers.TowerBuilders
{
    public class AITowerBuilder : TowerBuilder
    {
        public const string SimulationInputFileName = "simulation-input-defender.json";

        public enum DefenderDecisionAlgorithm
        {
            OriginalGa = 0,
            NsgaII = 1,
            LegacyGa = 2,
            Greedy = 3
        }

        private static readonly Dictionary<TowerType, double> TargetShares = new Dictionary<TowerType, double>
        {
            [TowerType.BallistaTower] = 0.15,
            [TowerType.ArcherTower] = 0.3,
            [TowerType.CatapultTower] = 0.3,
            [TowerType.IceMageTower] = 0.25,
        };

        [SerializeField]
        private MapController mapController;
        private TilesMap tilesMap;

        [SerializeField]
        private WaveSpawner waveSpawner;

        [Header("Effect")]
        [SerializeField]
        private GameObject effectBuildingPrefab;
        [SerializeField]
        private float timeEffectBuilding;

        [Header("Decision")]
        [SerializeField]
        private DefenderDecisionAlgorithm decisionAlgorithm = DefenderDecisionAlgorithm.OriginalGa;
        [SerializeField, Range(0.1f, 1f)]
        private float decisionTimeShare = 0.9f;
        [SerializeField]
        private bool useSeed = false;
        [SerializeField]
        private int seed = 0;
        [SerializeField, Range(0f, 1f)]
        private float balanceWeight = 1f;
        [SerializeField, Min(0f)]
        private float fireDensityWeight = 1f;

        [Header("NSGA-II")]
        [SerializeField, Range(0f, 1f)]
        private float difficultyLevel = 1.0f;
        [SerializeField, Range(0f, 1f)]
        private float minDifficultyLevel = 0.9f;
        [SerializeField]
        private bool adaptiveDifficulty = true;
        [SerializeField]
        private float targetCastleDamage = 10f;
        [SerializeField]
        private float regulatorStep = 0.05f;

        [Header("Expected wave")]
        [SerializeField, Min(0)]
        private int waveNumbersForThreatForecast = 3;

        private TowersStorage towersStorage;
        private PathCoverageMap coverageMap;
        private IDefenceDecisionStrategy decisionStrategy;
        private DefenderSnapshot decisionSnapshot;
        private Task<List<DefenderGene>> decisionTask;
        private int? castleHealthAtLastDecision;

        private int countBuildedTowerList = 0;


        private void Start()
        {
            towersStorage = Resources.Load<TowersStorage>($"{nameof(TowersStorage)}");
            tilesMap = mapController.TilesMap;

            coverageMap = PathCoverageMap.Build(tilesMap, towersStorage.Towers.Select(tower => tower.GetComponent<Tower>()));
            decisionStrategy = CreateStrategy();
        }

        private void Update()
        {
            if (decisionTask != null)
            {
                if (!decisionTask.IsCompleted)
                    return;

                BuildTowers(TakeDecision());
                decisionTask = null;
                countBuildedTowerList++;
                return;
            }

            if (waveSpawner.WaveFinished() && countBuildedTowerList != waveSpawner.WaveNumber)
                StartDecision();
        }


        private IDefenceDecisionStrategy CreateStrategy()
        {
            TimeSpan timeLimit = TimeSpan.FromSeconds(waveSpawner.TimeBetweenWaves * decisionTimeShare);
            int? strategySeed = useSeed ? seed : (int?)null;

            switch (decisionAlgorithm)
            {
                case DefenderDecisionAlgorithm.NsgaII:
                    return new NsgaIIDefenceStrategy(timeLimit, strategySeed, balanceWeight, fireDensityWeight, difficultyLevel, minDifficultyLevel, adaptiveDifficulty, targetCastleDamage, regulatorStep);
                case DefenderDecisionAlgorithm.LegacyGa:
                    return new LegacyGaDefenceStrategy(timeLimit, strategySeed, balanceWeight, fireDensityWeight);
                case DefenderDecisionAlgorithm.Greedy:
                    return new GreedyDefence(balanceWeight, fireDensityWeight);
                default:
                    return new OriginalGaDefenceStrategy(timeLimit, strategySeed, balanceWeight, fireDensityWeight);
            }
        }

        private void StartDecision()
        {
            ReportCastleDamage();

            List<DefenderGene> builtTowers = CollectBuiltTowers();
            DefenderSnapshot snapshot = new DefenderSnapshot(
                unlockScheduleController.UnlockedTowers,
                unlockScheduleController.TowerPrices,
                CollectFreeCells(),
                builtTowers,
                unlockScheduleController.UnlockedUnits,
                TargetShares,
                coverageMap,
                Mathf.Max(0, PlayerStats.MoneyDefender),
                CreateSimulationRequest(builtTowers),
                ExpectedWave());

            if (snapshot.CandidateGenes.Count == 0)
            {
                countBuildedTowerList++;
                return;
            }

            decisionSnapshot = snapshot;
            decisionTask = Task.Run(() => decisionStrategy.Decide(snapshot));
        }

        private SimulationRequest CreateSimulationRequest(IEnumerable<DefenderGene> builtTowers)
        {
            string inputPath = SimulationInputDataWriter.Write(tilesMap, PathGraphHolder.Graph, waveSpawner, SimulationInputFileName);
            return new SimulationRequest(inputPath, builtTowers.Select(SimulationTowerPlacementConverter.Of), int.MaxValue, waveSpawner.WaveNumber);
        }

        private List<UnitType> ExpectedWave()
        {
            List<UnitType> wave = waveSpawner.LastWave.ToList();
            List<UnitType> missingUnitTypes = unlockScheduleController.UnlockedUnitsAt(waveSpawner.WaveNumber + waveNumbersForThreatForecast).Where(unit => !wave.Contains(unit)).ToList();
            if (missingUnitTypes.Count == 0)
                return wave;

            int waveCost = Mathf.Max(wave.Sum(unit => unlockScheduleController.UnitPrices.TryGetValue(unit, out int price) ? price : 0), PlayerStats.MoneyAttacker);
            int costPerMissingUnit = waveCost / (missingUnitTypes.Count + (wave.Count > 0 ? 1 : 0));
            foreach (UnitType unit in missingUnitTypes)
                wave.AddRange(Enumerable.Repeat(unit, Mathf.Max(1, costPerMissingUnit / unlockScheduleController.UnitPrices[unit])));
            return wave;
        }

        private List<DefenderGene> TakeDecision()
        {
            if (decisionTask.IsFaulted)
            {
                Debug.LogException(decisionTask.Exception);
                return GreedyDefence.Build(decisionSnapshot, balanceWeight, fireDensityWeight);
            }

            Debug.Log($"Defence {waveSpawner.WaveNumber}: {decisionStrategy.LastReport}");
            return decisionTask.Result;
        }

        private void ReportCastleDamage()
        {
            int castleHealth = CastleHealth();
            if (castleHealthAtLastDecision.HasValue)
                decisionStrategy.OnWaveFinished(Mathf.Max(0, castleHealthAtLastDecision.Value - castleHealth));
            castleHealthAtLastDecision = castleHealth;
        }

        private void BuildTowers(IEnumerable<DefenderGene> towers)
        {
            foreach (DefenderGene tower in towers)
                BuildTower(tower.Type, tilesMap.GetTowerTileAt(tower.Cell.x, tower.Cell.y));
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

        private List<Vector2Int> CollectFreeCells()
        {
            var freeCells = new List<Vector2Int>();
            for (int row = 0; row < tilesMap.Size; row++)
            {
                for (int column = 0; column < tilesMap.Size; column++)
                {
                    TowerTile towerTile = tilesMap.GetTowerTileAt(row, column);
                    if (towerTile != null && towerTile.Tower == null)
                        freeCells.Add(new Vector2Int(row, column));
                }
            }
            return freeCells;
        }

        private List<DefenderGene> CollectBuiltTowers()
        {
            var builtTowers = new List<DefenderGene>();
            for (int row = 0; row < tilesMap.Size; row++)
            {
                for (int column = 0; column < tilesMap.Size; column++)
                {
                    TowerTile towerTile = tilesMap.GetTowerTileAt(row, column);
                    if (towerTile != null && towerTile.Tower != null)
                        builtTowers.Add(new DefenderGene(towerTile.Tower.GetComponent<Tower>().Type, new Vector2Int(row, column)));
                }
            }
            return builtTowers;
        }

        private static int CastleHealth()
        {
            GameObject castle = GameObject.FindGameObjectWithTag(EndBuilding.endBuildingTag);
            return castle == null ? 0 : castle.GetComponent<EndBuilding>().Health;
        }
    }

}
