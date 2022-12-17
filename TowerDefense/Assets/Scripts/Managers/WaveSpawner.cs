using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WaveSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private Transform enemyPrefab;


    [Header("Attributes")]
    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private float timeBetweenWaves = 5f;
    private float timeToNextSpawn = 1f;

    private int waveNumber = 1;

    [Header("UI")]
    [SerializeField]
    private TMPro.TextMeshProUGUI waveNumberText;
    [SerializeField]
    private TMPro.TextMeshProUGUI timeToNextWaveText;

    void Update()
    {
        if (timeToNextSpawn <= 0f)
        {
            StartCoroutine(SpawnWave());
            timeToNextSpawn = timeBetweenWaves;
        }
        timeToNextSpawn -= Time.deltaTime;

        timeToNextSpawn = Mathf.Clamp(timeToNextSpawn, 0f, Mathf.Infinity);
        timeToNextWaveText.text = "Next: " + string.Format("{0:00:00}", timeToNextSpawn);
    }

    IEnumerator SpawnWave()
    {
        waveNumberText.text = "Wave: " + (waveNumber).ToString();

        for (int i = 0; i < waveNumber; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.5f);
        }
        waveNumber++;
    }

    void SpawnEnemy()
    {
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
