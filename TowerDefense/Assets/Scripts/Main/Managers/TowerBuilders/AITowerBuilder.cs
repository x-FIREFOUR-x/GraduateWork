using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Map;
using TowerDefense.Main.Map.Buildings;
using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Managers.WaveSpawners;
using TowerDefense.Main.Towers;
using TowerDefense.Opponents.Defender.State;
using TowerDefense.Opponents.Defender.Strategies;


namespace TowerDefense.Main.Managers.TowerBuilders
{
    public class AITowerBuilder : MonoBehaviour
    {
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
        [SerializeField]
        private float timeLimitSeconds = 1f;
        [SerializeField]
        private bool useSeed = false;
        [SerializeField]
        private int seed = 0;
        [SerializeField, Min(0f)]
        private float balanceWeight = 0.3f;
        [SerializeField, Min(0f)]
        private float fireDensityWeight = 1f;

        [Header("NSGA-II")]
        [SerializeField, Range(0f, 1f)]
        private float difficultyLevel = 0.7f;
        [SerializeField]
        private bool adaptiveDifficulty = true;
        [SerializeField]
        private float targetCastleDamage = 10f;
        [SerializeField]
        private float regulatorStep = 0.02f;

        private TowersStorage towersStorage;
        private PathCoverageMap coverageMap;
        private IDefenceDecisionStrategy decisionStrategy;
        private DefenderSnapshot decisionSnapshot;
        private Task<List<DefenderGene>> decisionTask;
        private int? castleHealthAtLastDecision;

        private List<TowerType> availableTowersTypes;
        private Dictionary<TowerType, int> availableTowersPrices;

        private int countBuildedTowerList = 0;


        private void Start()
        {
            towersStorage = Resources.Load<TowersStorage>($"{nameof(TowersStorage)}");
            tilesMap = mapController.TilesMap;

            availableTowersTypes = new List<TowerType>();
            availableTowersPrices = new Dictionary<TowerType, int>();

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
            TimeSpan timeLimit = TimeSpan.FromSeconds(timeLimitSeconds);
            int? strategySeed = useSeed ? seed : (int?)null;

            switch (decisionAlgorithm)
            {
                case DefenderDecisionAlgorithm.NsgaII:
                    return new NsgaIIDefenceStrategy(timeLimit, strategySeed, fireDensityWeight, difficultyLevel, adaptiveDifficulty, targetCastleDamage, regulatorStep);
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
            UpdateAvailableTower();
            ReportCastleDamage();

            DefenderSnapshot snapshot = new DefenderSnapshot(
                availableTowersTypes,
                availableTowersPrices,
                CollectFreeCells(),
                CollectBuiltTowers(),
                waveSpawner.AvailableUnits,
                TargetShares,
                coverageMap,
                Mathf.Max(0, PlayerStats.MoneyDefender));

            if (snapshot.CandidateGenes.Count == 0)
            {
                countBuildedTowerList++;
                return;
            }

            decisionSnapshot = snapshot;
            decisionTask = Task.Run(() => decisionStrategy.Decide(snapshot));
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

        private void UpdateAvailableTower()
        {
            switch (countBuildedTowerList)
            {
                case 0:
                    AddAvailableTower(TowerType.BallistaTower);
                    break;
                case 2:
                    AddAvailableTower(TowerType.ArcherTower);
                    break;
                case 4:
                    AddAvailableTower(TowerType.CatapultTower);
                    break;
                case 6:
                    AddAvailableTower(TowerType.IceMageTower);
                    break;

                default:
                    break;
            }
        }

        private void AddAvailableTower(TowerType towerType)
        {
            availableTowersTypes.Add(towerType);
            availableTowersPrices[towerType] = towersStorage.Towers[(int)towerType].GetComponent<Tower>().Price;
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
