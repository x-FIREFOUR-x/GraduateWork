using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using TowerDefense.Main.Units;


namespace TowerDefense.Main.UI.UnitMenu
{
    public class UnitShopComponent : MonoBehaviour
    {
        [SerializeField]
        private Unit unit;

        private int count = 0;


        [SerializeField]
        private UnitShopMenu unitShopMenu;


        [Header("Text Components Settings")]
        [SerializeField]
        private TMPro.TextMeshProUGUI textName;
        [SerializeField]
        private TMPro.TextMeshProUGUI textPrice;
        [SerializeField]
        private TMPro.TextMeshProUGUI textCount;

        [SerializeField]
        private Color colorText;


        [Header("Image Button Settings")]
        [SerializeField]
        private GameObject characterUnit;
        [SerializeField]
        private GameObject textCharacter;
        [SerializeField]
        private Color colorCharacterText;
        [SerializeField]
        private Color colorHighlightText;


        [Header("Add and Sub Buttons Settings")]
        [SerializeField]
        private Button buttonSub;
        [SerializeField]
        private Button button5Sub;
        [SerializeField]
        private Button buttonAdd;
        [SerializeField]
        private Button button5Add;

        [SerializeField]
        private Color failColorButtonsAddAndSub;
        private Color colorButtonsAddAndSub;

        [SerializeField]
        private float timeShowFail;


        public void Initialize()
        {
            textName.color = colorText;

            textPrice.color = colorText;
            textPrice.text = unit.Price.ToString();

            textCount.color = colorText;
            textCount.text = "0";

            colorButtonsAddAndSub = buttonAdd.GetComponent<Image>().color;
            buttonAdd.onClick.AddListener(() => AddUnit(buttonAdd, 1));
            button5Add.onClick.AddListener(() => AddUnit(button5Add, 5));
            buttonSub.onClick.AddListener(() => SubUnit(buttonSub, 1));
            button5Sub.onClick.AddListener(() => SubUnit(button5Sub, 5));

            InitializetextCharacters();
        }

        private void InitializetextCharacters()
        {
            textCharacter.GetComponent<TMPro.TextMeshProUGUI>().color = colorCharacterText;
            textCharacter.GetComponent<TMPro.TextMeshProUGUI>().text =
                "Health " + Highlight(unit.StartHealth.ToString()) + "\n" +
                "Speed " + Highlight(unit.StartSpeed.ToString()) + "\n" +
                "Damage " + Highlight(unit.Damage.ToString());
        }

        private string Highlight(string value)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(colorHighlightText) + ">" + value + "</color>";
        }

        public void ChangeImageToCharacter()
        {
            if (characterUnit.activeSelf)
            {
                characterUnit.SetActive(false);
                textCharacter.SetActive(false);
            }
            else
            {
                characterUnit.SetActive(true);
                textCharacter.SetActive(true);
            }

        }

        public void ClearCount()
        {
            count = 0;
            textCount.text = count.ToString();
        }

        private void AddUnit(Button clickedButton, int countAdd)
        {
            if (unitShopMenu.AddUnit(unit.UnitType, unit.Price, countAdd))
            {
                count += countAdd;
                textCount.text = count.ToString();
            }
            else
            {
                clickedButton.GetComponent<Image>().color = failColorButtonsAddAndSub;
                StartCoroutine(RestoringButtonAfterFail(timeShowFail, clickedButton));
            }
        }

        private void SubUnit(Button clickedButton, int countSub)
        {

            if (unitShopMenu.SubUnit(unit.UnitType, unit.Price, count, countSub))
            {
                count -= countSub;
                textCount.text = count.ToString();
            }
            else
            {
                clickedButton.GetComponent<Image>().color = failColorButtonsAddAndSub;
                StartCoroutine(RestoringButtonAfterFail(timeShowFail, clickedButton));
            }
        }

        private IEnumerator RestoringButtonAfterFail(float deleyTime, Button button)
        {
            yield return new WaitForSeconds(deleyTime);
            button.GetComponent<Image>().color = colorButtonsAddAndSub;
        }
    }

}
