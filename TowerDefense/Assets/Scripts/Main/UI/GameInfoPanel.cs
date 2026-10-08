using UnityEngine;
using UnityEngine.UI;

using TMPro;

using TowerDefense.Main.Managers.WaveSpawners;
using TowerDefense.Main.Map.Buildings;


namespace TowerDefense.Main.UI
{
    public class GameInfoPanel : MonoBehaviour
    {
        private const string battleText = "The battle is joined";
        private const string countdownText = "Next wave";

        [Header("Wave")]
        [SerializeField]
        private TextMeshProUGUI waveNumberText;

        [Header("Timer")]
        [SerializeField]
        private TextMeshProUGUI timerLabelText;
        [SerializeField]
        private TextMeshProUGUI timerValueText;
        [SerializeField]
        private GameObject hourglassIcon;
        [SerializeField]
        private GameObject battleIcon;
        [SerializeField]
        private Image countdownFill;

        [Header("Health")]
        [SerializeField]
        private Image healthFill;
        [SerializeField]
        private Image healthTrail;
        [SerializeField]
        private RectTransform healthGlow;
        [SerializeField]
        private TextMeshProUGUI healthText;

        [SerializeField]
        private float lowHealthShare = 0.25f;
        [SerializeField]
        private float trailDelay = 0.4f;
        [SerializeField]
        private float trailSpeed = 0.6f;
        private float trailShare = 1f;
        private float trailHoldUntil;

        [Header("Colors")]
        [SerializeField]
        private Color textColor = new(0.953f, 0.902f, 0.753f);
        [SerializeField]
        private Color battleTextColor = new(0.965f, 0.788f, 0.659f);
        [SerializeField]
        private Color lowHealthTextColor = new(1f, 0.604f, 0.478f);
        [SerializeField]
        private Color countdownColor = new(0.886f, 0.757f, 0.447f);
        [SerializeField]
        private Color battleCountdownColor = new(0.769f, 0.384f, 0.165f);

        private WaveSpawner waveSpawner;
        private EndBuilding endBuilding;


        private void Update()
        {
            if (waveSpawner == null)
                waveSpawner = FindAnyObjectByType<WaveSpawner>();
            if (endBuilding == null)
                endBuilding = FindAnyObjectByType<EndBuilding>();

            if (waveSpawner != null)
                ShowWave();
            if (endBuilding != null)
                ShowHealth();
        }


        private void ShowWave()
        {
            waveNumberText.text = waveSpawner.WaveNumber.ToString();

            bool isBattle = !waveSpawner.WaveFinished();

            hourglassIcon.SetActive(!isBattle);
            battleIcon.SetActive(isBattle);
            timerValueText.gameObject.SetActive(!isBattle);

            timerLabelText.text = isBattle ? battleText : countdownText;
            timerLabelText.color = isBattle ? battleTextColor : textColor;

            int seconds = Mathf.CeilToInt(waveSpawner.timeToNextSpawn);
            timerValueText.text = $"{seconds / 60}:{seconds % 60:00}";

            float timeBetweenWaves = waveSpawner.TimeBetweenWaves;
            countdownFill.fillAmount = isBattle || timeBetweenWaves <= 0f
                ? 1f
                : Mathf.Clamp01(1f - waveSpawner.timeToNextSpawn / timeBetweenWaves);
            countdownFill.color = isBattle ? battleCountdownColor : countdownColor;
        }

        private void ShowHealth()
        {
            float share = endBuilding.MaxHealth > 0 ? Mathf.Clamp01((float)endBuilding.Health / endBuilding.MaxHealth) : 0f;

            if (share >= trailShare)
            {
                trailShare = share;
            }
            else if (healthFill.fillAmount > share)
            {
                trailHoldUntil = Time.time + trailDelay;
            }
            else if (Time.time >= trailHoldUntil)
            {
                trailShare = Mathf.MoveTowards(trailShare, share, trailSpeed * Time.deltaTime);
            }

            healthFill.fillAmount = share;
            healthTrail.fillAmount = trailShare;

            healthGlow.gameObject.SetActive(share > 0f);
            healthGlow.anchorMin = new Vector2(share, healthGlow.anchorMin.y);
            healthGlow.anchorMax = new Vector2(share, healthGlow.anchorMax.y);

            healthText.text = endBuilding.Health.ToString();
            healthText.color = share <= lowHealthShare ? lowHealthTextColor : textColor;
        }
    }

}
