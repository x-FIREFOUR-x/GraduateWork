using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField]
    private HeuristicsCalculator calculator;

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

    public void Initialize(Transform spawn)
    {
        spawnPoint = spawn;
    }

    void Update()
    {
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
        for (int i = 0; i < waveNumber + 1; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }
        waveNumber++;
    }

    private void SpawnEnemy()
    {
        Instantiate(standardEnemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
