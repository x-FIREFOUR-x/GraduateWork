using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using TowerDefense.Main.Units;
using TowerDefense.Algorithms.GeneticAlgorithm;
using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;
using TowerDefense.Algorithms.GeneticAlgorithm.Persons;
using TowerDefense.Algorithms.GeneticAlgorithm.Mutations;


namespace TowerDefense.Main.Managers.WaveSpawners
{

    public class AIWaveSpawner: WaveSpawner
    {
        private GeneticAlgorithm<Person<UnitType>, UnitType> algoCreateWave;

        private bool waveGenerated = false;
        private bool isTaskRunning = false;
        private Task<List<UnitType>> taskGenerated;


        public override void Initialize(Transform spawn)
        {
            base.Initialize(spawn);

            MutationSelector mutationSelector = new MutationSelector(new List<Mutation> {
                new Mutation(MutationType.None, 0),
                new Mutation(MutationType.RandomRegenerateRange, 0.5f),
                new Mutation(MutationType.ChangeRangeOneType, 0.5f)
            });
            algoCreateWave = new GeneticAlgorithm<Person<UnitType>, UnitType>(new PersonFactory<UnitType>(), new UnitsHeuristicsCalculator(), mutationSelector);

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

            if (timeToNextSpawn <= 0f && waveGenerated)
            {
                StartCoroutine(SpawnWave());
                timeToNextSpawn = timeBetweenWaves;
            }
            timeToNextSpawn -= Time.deltaTime;

            timeToNextSpawn = Mathf.Clamp(timeToNextSpawn, 0f, Mathf.Infinity);

            if (!waveGenerated && !isTaskRunning)
            {
                UpdateAvailableUnit();

                UnitsHeuristicsCalculator.instance.UpdateCurrentTower();

                isTaskRunning = true;
                taskGenerated = GeneratedWave();
            }
        }


        private IEnumerator SpawnWave()
        {
            waveGenerated = false;
            nextWave = taskGenerated.Result;
            nextWave = SortBalanceWave(nextWave);

            OperationPrevStartWave();

            for (int i = 0; i < nextWave.Count; i++)
            {
                SpawnUnit(nextWave[i]);
                yield return new WaitForSeconds(0.5f);
            }

            AllCurrentWaveUnitsSpawned = true;
        }

        private Task<List<UnitType>> GeneratedWave()
        {
            return Task<List<UnitType>>.Run(() =>
            {
                var result = algoCreateWave.SearchKnapsac(availableUnit, availablePrices, UnitsHeuristicsCalculator.instance.MoneyForWave());
                OperationsPostGeneratingWave();
                return result;
            });

        }

        private List<UnitType> SortBalanceWave(List<UnitType> unitsWave)
        {
            if (WaveNumber == 1)
            {
                return unitsWave.GetRange(0, 3);
            }
            //unitsWave.Sort();
            return unitsWave;
        }

        private void OperationsPostGeneratingWave()
        {
            waveGenerated = true;
            isTaskRunning = false;
        }
    }

}
