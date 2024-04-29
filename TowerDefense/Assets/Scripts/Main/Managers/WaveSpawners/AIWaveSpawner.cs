using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using TowerDefense.Main.Enemies;
using TowerDefense.Algorithms.GeneticAlgorithm;
using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;
using TowerDefense.Algorithms.GeneticAlgorithm.Persons;


namespace TowerDefense.Main.Managers.WaveSpawners
{

    public class AIWaveSpawner: WaveSpawner
    {
        private GeneticAlgorithm<Person<EnemyType>, EnemyType> algoCreateWave;

        private bool waveGenerated = false;
        private bool isTaskRunning = false;
        private Task<List<EnemyType>> taskGenerated;


        public override void Initialize(Transform spawn)
        {
            base.Initialize(spawn);

            algoCreateWave = new GeneticAlgorithm<Person<EnemyType>, EnemyType>(new PersonFactory<EnemyType>(), new EnemiesHeuristicsCalculator());

            availableEnemy = new List<EnemyType>();
            availablePrices = new Dictionary<EnemyType, int>();
        }


        void Update()
        {
            if (WaveNotFinished())
            {
                return;
            }
            else
            {
                if (!isEnrolledhWave)
                {
                    WaveNumber++;
                    PlayerStats.IncreaseMoneyAfterWave();
                    isEnrolledhWave = true;
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
                UpdateGameStats();
                UpdateAvailableEnemy();

                EnemiesHeuristicsCalculator.instance.UpdateCurrentTower();

                isTaskRunning = true;
                taskGenerated = GeneratedWave();
            }
        }


        private IEnumerator SpawnWave()
        {
            waveGenerated = false;
            nextWave = taskGenerated.Result;
            nextWave = SortBalanceWave(nextWave);

            isEnrolledhWave = false;

            for (int i = 0; i < nextWave.Count; i++)
            {
                SpawnEnemy(nextWave[i]);
                yield return new WaitForSeconds(0.5f);
            }
        }

        private Task<List<EnemyType>> GeneratedWave()
        {
            return Task<List<EnemyType>>.Run(() =>
            {
                var result = algoCreateWave.SearchKnapsac(availableEnemy, availablePrices, EnemiesHeuristicsCalculator.instance.MoneyForWave());
                OperationsPostGeneratingWave();
                return result;
            });

        }

        private List<EnemyType> SortBalanceWave(List<EnemyType> enemiesWave)
        {
            if (WaveNumber == 1)
            {
                return enemiesWave.GetRange(0, 3);
            }
            //enemiesWave.Sort();
            return enemiesWave;
        }

        private void OperationsPostGeneratingWave()
        {
            UpdateMoneyByWave();

            waveGenerated = true;
            isTaskRunning = false;
        }
    }

}
