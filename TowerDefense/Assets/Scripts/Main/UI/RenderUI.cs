using UnityEngine;

public class RenderUI : MonoBehaviour
{
    [Header("Text field")]

    [SerializeField]
    private TMPro.TextMeshProUGUI countMoney;
    [SerializeField]
    private TMPro.TextMeshProUGUI healthText;
    [SerializeField]
    private TMPro.TextMeshProUGUI waveNumberText;
    [SerializeField]
    private TMPro.TextMeshProUGUI timeToNextWaveText;

    [Header("Prefab")]
    [SerializeField]
    private GameObject waveSpawnerPrefab;
    [SerializeField]
    private GameObject endBuildingPrefab;

    void Update()
    {
        WaveSpawner waveSpawner = waveSpawnerPrefab.GetComponent<WaveSpawner>();
        timeToNextWaveText.text = "Next: " + string.Format("{0:00:00}", waveSpawner.timeToNextSpawn);
        waveNumberText.text = "Wave: " + (waveSpawner.WaveNumber).ToString();

        countMoney.text = PlayerStats.GetPlayerMoney().ToString() + "$";

        int health = endBuildingPrefab.GetComponent<EndBuilding>().health;
        healthText.text = "Health: " + health.ToString();
        
    }
}
