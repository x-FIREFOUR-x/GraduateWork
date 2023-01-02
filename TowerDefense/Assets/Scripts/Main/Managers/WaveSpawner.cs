using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    private GeneticAlgorithm algoCreateWave;

    [Header("Enemy Prefabs")]
    [SerializeField]
    private Transform standardEnemyPrefab;
    [SerializeField]
    private Transform fastEnemyPrefab;
    [SerializeField]
    private Transform tankEnemyPrefab;

    private Transform spawnPoint;

    [Header("Attributes")]
    [SerializeField]
    private float timeBetweenWaves = 5f;
    public float timeToNextSpawn { get; private set; } = 1f;

    public int waveNumber { get; private set; } = 0;

    private List<EnemyType> availableEnemy;
    Dictionary<EnemyType, int> availablePrices;

    bool isNotEnrolledhWave = false;

    public void Initialize(Transform spawn)
    {
        spawnPoint = spawn;
        algoCreateWave = new GeneticAlgorithm();

        availableEnemy = new();
        availablePrices = new();
    }

    void Update()
    {
        if (GameObject.FindGameObjectsWithTag(Enemy.enemyTag).Length > 0)
        {
            return;
        }
        else
        {
            if (isNotEnrolledhWave)
            {
                PlayerStats.AddMoney();
                isNotEnrolledhWave = false;
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
        UpdateAvailableEnemy();
        List<EnemyType> packEnemies = algoCreateWave.SearchKnapsac(availableEnemy, availablePrices);

        isNotEnrolledhWave = true;

        for (int i = 0; i < packEnemies.Count; i++)
        {
            SpawnEnemy(packEnemies[i]);
            yield return new WaitForSeconds(0.5f);
        }
        waveNumber++;
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
            case 0:
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

}
