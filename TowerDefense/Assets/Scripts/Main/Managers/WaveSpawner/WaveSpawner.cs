using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class WaveSpawner : MonoBehaviour
{
    public Transform SpawnPoint { get; protected set; }

    [Header("Enemy Prefabs")]
    [SerializeField]
    protected Transform standardEnemyPrefab;
    [SerializeField]
    protected Transform fastEnemyPrefab;
    [SerializeField]
    protected Transform tankEnemyPrefab;

    [Header("Attributes")]
    [SerializeField]
    protected float timeBetweenWaves = 5f;
    public float timeToNextSpawn { get; protected set; } = 5f;

    public int waveNumber { get; protected set; } = 1;


    protected List<EnemyType> availableEnemy;
    protected Dictionary<EnemyType, int> availablePrices;

    protected List<EnemyType> nextWave;
    protected bool isEnrolledhWave = true;


    public abstract void Initialize(Transform spawn);


    protected bool WaveNotFinished()
    {
        return GameObject.FindGameObjectsWithTag(Enemy.enemyTag).Length > 0;
    }

    protected void SpawnEnemy(EnemyType enemyType)
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

    protected void UpdateAvailableEnemy()
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

    protected void UpdateGameStats()
    {
        if(waveNumber % 5 == 0)
        {
            standardEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
            fastEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
            tankEnemyPrefab.GetComponent<Enemy>().UpgradeHealth((float)0.25);
        }
    }
}
