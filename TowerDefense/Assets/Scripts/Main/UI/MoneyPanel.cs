using System.Globalization;

using UnityEngine;

using TMPro;

using TowerDefense.Main.Managers;


namespace TowerDefense.Main.UI
{
    public class MoneyPanel : MonoBehaviour
    {
        private static readonly NumberFormatInfo moneyFormat = new() { NumberGroupSeparator = " ", NumberGroupSizes = new[] { 3 } };

        [Header("UI Elements")]
        [SerializeField]
        private TextMeshProUGUI moneyText;
        [SerializeField]
        private TextMeshProUGUI deltaMoneyText;
        [SerializeField]
        private RectTransform pouchSprite;

        [Header("Animation Settings")]
        [SerializeField]
        private float flashMoneyTime = 0.8f;
        [SerializeField]
        private float deltaMoneyShowTime = 1.2f;
        [SerializeField]
        private float deltaMoneyRise = 8f;
        [SerializeField]
        private float pouchSizeIncreasePercent = 0.25f;
        [SerializeField]
        private float noAnimationTimeAfterSceneLoad = 0.5f;

        [Header("Colors")]
        [SerializeField]
        private Color textColor = new(0.953f, 0.902f, 0.753f);
        [SerializeField]
        private Color gainColor = new(0.659f, 0.878f, 0.478f);
        [SerializeField]
        private Color spendColor = new(1f, 0.604f, 0.478f);

        private int shownMoney;
        private bool isGain;
        private float changedAt = float.NegativeInfinity;
        private Vector2 deltaMoneyStartPosition;


        private void Awake()
        {
            deltaMoneyStartPosition = deltaMoneyText.rectTransform.anchoredPosition;
            shownMoney = PlayerStats.GetPlayerMoney();
            moneyText.text = Format(shownMoney);
        }

        private void Update()
        {
            int money = PlayerStats.GetPlayerMoney();
            if (money != shownMoney)
            {
                if (Time.timeSinceLevelLoad >= noAnimationTimeAfterSceneLoad)
                    ShowChange(money - shownMoney);

                shownMoney = money;
                moneyText.text = Format(money);
            }

            Animate();
        }


        private void ShowChange(int change)
        {
            isGain = change > 0;
            changedAt = Time.time;
            deltaMoneyText.text = (isGain ? "+" : "-") + Format(Mathf.Abs(change));
        }

        private void Animate()
        {
            float sinceChange = Time.time - changedAt;
            Color flashColor = isGain ? gainColor : spendColor;

            moneyText.color = Color.Lerp(flashColor, textColor, Mathf.Clamp01(sinceChange / flashMoneyTime));

            float rise = Mathf.Clamp01(sinceChange / deltaMoneyShowTime);
            deltaMoneyText.rectTransform.anchoredPosition = deltaMoneyStartPosition + Vector2.up * deltaMoneyRise * rise;
            flashColor.a = 1f - rise * rise;
            deltaMoneyText.color = flashColor;

            float pouchSizeIncrease = Mathf.Sin(Mathf.PI * Mathf.Clamp01(sinceChange / (flashMoneyTime * 0.5f))) * pouchSizeIncreasePercent;
            pouchSprite.localScale = isGain
                ? new Vector3(1f + pouchSizeIncrease, 1f + pouchSizeIncrease, 1f)
                : new Vector3(1f + pouchSizeIncrease * 0.6f, 1f - pouchSizeIncrease * 0.6f, 1f);
        }

        private static string Format(int money)
        {
            return money.ToString("#,0", moneyFormat);
        }
    }

}
