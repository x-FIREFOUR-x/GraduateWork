using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerWaveSpawner : WaveSpawner
{
    [SerializeField]
    private EnemyShopMenu enemyShopMenu;

    private bool WaveToBeStarted = false;

    public override void Initialize(Transform spawn)
    {
        base.Initialize(spawn);

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
            if (WaveToBeStarted)
            {
                WaveNumber++;
                PlayerStats.IncreaseMoneyAfterWave();
                UpdateGameStats();

                WaveToBeStarted = false;
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
        WaveToBeStarted = true;

        nextWave = enemyShopMenu.TakeEnemies();
        for (int i = 0; i < nextWave.Count; i++)
        {
            SpawnEnemy(nextWave[i]);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
