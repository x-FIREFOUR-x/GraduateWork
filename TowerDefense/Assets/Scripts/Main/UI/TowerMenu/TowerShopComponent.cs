using System;

using UnityEngine;
using UnityEngine.UI;

using TowerDefense.Main.Towers;


namespace TowerDefense.Main.UI.TowerMenu
{
    public class TowerShopComponent : MonoBehaviour
    {
        public bool IsSelected { get; private set; }

        [SerializeField]
        private Tower tower;

        [Header("Components")]
        [SerializeField]
        private Image background;
        [field: SerializeField]
        public Button ButtonSelect{ get; private set; }
        [SerializeField]
        private TMPro.TextMeshProUGUI textName;
        [SerializeField]
        private TMPro.TextMeshProUGUI textPrice;
        [SerializeField]
        private Button ButtonCharacter;
        [SerializeField]
        private GameObject selectedFrame;

        [Header("ButtonSelect Components")]
        [SerializeField]
        private GameObject characterTower;
        [SerializeField]
        private GameObject textCharacter;

        [Header("Colors")]
        [SerializeField]
        private Color colorSelected;
        [SerializeField]
        private Color colorUnselected;
        [SerializeField]
        private Color colorText;
        [SerializeField]
        private Color colorCharacterText;
        [SerializeField]
        private Color colorHighlightText;

        public void Initialize(int price, float sizeUnitTileInRange)
        {
            IsSelected = false;

            textName.color = colorText;
            textPrice.color = colorText;
            ButtonCharacter.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = colorText;

            textPrice.text = price.ToString();

            InitializetextCharacters(sizeUnitTileInRange);
        }

        private void InitializetextCharacters(float sizeUniSizeUnitTileInRangetTile)
        {
            float range = (tower.ShootRange - sizeUniSizeUnitTileInRangetTile / 2) / sizeUniSizeUnitTileInRangetTile ;

            textCharacter.GetComponent<TMPro.TextMeshProUGUI>().color = colorCharacterText;
            textCharacter.GetComponent<TMPro.TextMeshProUGUI>().text =
                "Bullets " + Highlight(tower.CountProjectileEntitys.ToString()) + "\n" +
                "Range " + Highlight(MathF.Round(range, 1).ToString()) + "\n" +
                "Cooldown " + Highlight(tower.TimeBetweenShoots.ToString()) + "\n" +
                "DPS " + Highlight(MathF.Round(tower.DamageInSecond(), 1).ToString());
        }

        private string Highlight(string value)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(colorHighlightText) + ">" + value + "</color>";
        }

        public void ChangeImageToCharacter()
        {
            if (characterTower.activeSelf)
            {
                characterTower.SetActive(false);
                textCharacter.SetActive(false);
            }
            else
            {
                characterTower.SetActive(true);
                textCharacter.SetActive(true);
            }

        }

        public void SetComponentSelected(bool isSelected)
        {
            IsSelected = isSelected;
            selectedFrame.SetActive(isSelected);
            if (IsSelected)
            {
                background.color = colorSelected;
            }
            else
            {
                background.color = colorUnselected;
            }
        }
    }

}
