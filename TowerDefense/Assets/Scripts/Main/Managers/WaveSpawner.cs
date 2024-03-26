using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WaveSpawner : MonoBehaviour
{
    public Transform SpawnPoint { get; private set; }

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
    [field:SerializeField]
    public float timeToNextSpawn { get; private set; } = 5f;

    public int waveNumber { get; private set; } = 1;


    private GeneticAlgorithm algoCreateWave;

    private List<EnemyType> availableEnemy;
    private Dictionary<EnemyType, int> availablePrices;

    private List<EnemyType> nextWave;
    private bool waveGenerated = false;
    private bool isTaskRunning = false;
    private Task<List<EnemyType>> taskGenerated;

    private bool isEnrolledhWave = true;


    public void Initialize(Transform spawn)
    {
        SpawnPoint = spawn;

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

    private bool WaveNotFinished()
    {
        return GameObject.FindGameObjectsWithTag(Enemy.enemyTag).Length > 0;
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

    private void SpawnEnemy(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Standard:
                Instantiate(standardEnemyPrefab, SpawnPoint.position, SpawnPoint.rotation);
                break;
            case EnemyType.Fast:
                Instantiate(fastEnemyPrefab, SpawnPoint.position, SpawnPoint.rotation);
                break;
            case EnemyType.Tank:
                Instantiate(tankEnemyPrefab, SpawnPoint.position, SpawnPoint.rotation);
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
        //enemiesWave.Sort();
        return enemiesWave;
    }

    private void UpdateGameStats()
    {
        if(waveNumber % 5 == 0)
        {
            standardEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
            fastEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
            tankEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
        }
    }

    private void OperationsPostGeneratingWave()
    {
        int numberNextWave = waveNumber + 1;
        if (numberNextWave % 5 == 0)
        {
            PlayerStats.IncreaseMoneyByWave(2);
        }
        PlayerStats.AddTotalMoney();

        waveGenerated = true;
        isTaskRunning = false;
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
}
