using UnityEngine;

using TowerDefense.Main.Managers;
using TowerDefense.Main.Managers.WaveSpawners;
using TowerDefense.Main.Map.Buildings;


namespace TowerDefense.Main.UI
{
    public class RenderUI : MonoBehaviour
    {
        [SerializeField]
        private WaveSpawner waveSpawner;
        [SerializeField]
        private EndBuilding endBuilding;

        [Header("Text field")]
        [SerializeField]
        private TMPro.TextMeshProUGUI countMoney;
        [SerializeField]
        private TMPro.TextMeshProUGUI healthText;
        [SerializeField]
        private TMPro.TextMeshProUGUI waveNumberText;
        [SerializeField]
        private TMPro.TextMeshProUGUI timeToNextWaveText;


        void Update()
        {
            timeToNextWaveText.text = "Next: " + string.Format("{0:00:00}", waveSpawner.timeToNextSpawn);
            waveNumberText.text = "Wave: " + (waveSpawner.WaveNumber).ToString();

            countMoney.text = PlayerStats.GetPlayerMoney().ToString() + "$";

            healthText.text = "Health: " + endBuilding.Health.ToString();
        }

    }

}
