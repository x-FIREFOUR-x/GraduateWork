using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    private Transform spawnPoint;

    [Header("Enemy Prefabs")]
    [SerializeField]
    private Transform standardEnemyPrefab;
    [SerializeField]
    private Transform fastEnemyPrefab;
    [SerializeField]
    private Transform tankEnemyPrefab;

    [Header("Attributes")]
    [SerializeField]
    private float timeBetweenWaves = 5f;
    public float timeToNextSpawn { get; private set; } = 5f;

    public int waveNumber { get; private set; } = 1;


    private GeneticAlgorithm algoCreateWave;

    private List<EnemyType> availableEnemy;
    private Dictionary<EnemyType, int> availablePrices;

    private bool isEnrolledhWave = true;


    public void Initialize(Transform spawn)
    {
        spawnPoint = spawn;

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
                waveNumber++;
                PlayerStats.AddMoney();
                isEnrolledhWave = true;
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

    private bool WaveNotFinished()
    {
        return GameObject.FindGameObjectsWithTag(Enemy.enemyTag).Length > 0;
    }

    private IEnumerator SpawnWave()
    {
        UpdateEnemiesStats();

        UpdateAvailableEnemy();
        List<EnemyType> enemiesWave = algoCreateWave.SearchKnapsac(availableEnemy, availablePrices);
        enemiesWave = SortBalanceWave(enemiesWave);

        isEnrolledhWave = false;

        for (int i = 0; i < enemiesWave.Count; i++)
        {
            SpawnEnemy(enemiesWave[i]);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SpawnEnemy(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Standard:
                Instantiate(standardEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
                break;
            case EnemyType.Fast:
                Instantiate(fastEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
                break;
            case EnemyType.Tank:
                Instantiate(tankEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
                break;
            default:
                break;
        }
    }

    private void UpdateAvailableEnemy()
    {
        switch (waveNumber)
        {
            case 1:
                availableEnemy.Add(EnemyType.Standard);
                availablePrices[EnemyType.Standard] = standardEnemyPrefab.GetComponent<Enemy>().Price;
                break;
            case 3:
                availablePrices[EnemyType.Fast] = fastEnemyPrefab.GetComponent<Enemy>().Price;
                availableEnemy.Add(EnemyType.Fast);
                break;
            case 5:
                availablePrices[EnemyType.Tank] = tankEnemyPrefab.GetComponent<Enemy>().Price;
                availableEnemy.Add(EnemyType.Tank);
                break;

            default:
                break;
        }
    }

    private List<EnemyType> SortBalanceWave(List<EnemyType> enemiesWave)
    {
        if (waveNumber == 1)
        {
            return enemiesWave.GetRange(0, 3);
        }

        List<EnemyType> newEnemiesWave = new();

        int countStandard = 0;
        int countFast = 0;
        int countTank = 0;

        for (int i = 0; i < enemiesWave.Count; i++)
        {
            switch (enemiesWave[i])
            {
                case EnemyType.Standard:
                    countStandard++;
                    break;
                case EnemyType.Fast:
                    countFast++;
                    break;
                case EnemyType.Tank:
                    countTank++;
                    break;
                default:
                    break;
            }
        }

        for (int i = 0; i < countTank; i++)
            newEnemiesWave.Add(EnemyType.Tank);

        for (int i = 0; i < countStandard; i++)
            newEnemiesWave.Add(EnemyType.Standard);

        for (int i = 0; i < countFast; i++)
            newEnemiesWave.Add(EnemyType.Fast);

        return newEnemiesWave;
    }

    private void UpdateEnemiesStats()
    {
        if(waveNumber % 5 == 0)
        {
            standardEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
            fastEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
            tankEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
        }
    }
}
