using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AIWaveSpawner : WaveSpawner
{
    private GeneticAlgorithm algoCreateWave;

    private bool waveGenerated = false;
    private bool isTaskRunning = false;
    private Task<List<EnemyType>> taskGenerated;


    public override void Initialize(Transform spawn) 
    {
        base.Initialize(spawn);

        algoCreateWave = new GeneticAlgorithm();

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

            HeuristicsCalculator.instance.UpdateCurrentTower();

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
            var result = algoCreateWave.SearchKnapsac(availableEnemy, availablePrices);
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
        int numberNextWave = WaveNumber + 1;
        if (numberNextWave % 5 == 0)
        {
            PlayerStats.IncreaseMoneyByWave(2);
        }

        waveGenerated = true;
        isTaskRunning = false;
    }
}
