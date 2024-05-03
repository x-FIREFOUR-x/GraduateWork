using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using TowerDefense.Main.Enemies;


namespace TowerDefense.Main.UI.EnemyMenu
{
    public class EnemyShopComponent : MonoBehaviour
    {
        [SerializeField]
        private Enemy enemy;

        private int count = 0;


        [SerializeField]
        private EnemyShopMenu enemyShopMenu;


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
        private GameObject characterEnemy;
        [SerializeField]
        private GameObject textCharacter;
        [SerializeField]
        private Color colorCharacterText;


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
            textPrice.text = enemy.Price.ToString() + "$";

            textCount.color = colorText;
            textCount.text = "0";

            colorButtonsAddAndSub = buttonAdd.GetComponent<Image>().color;
            buttonAdd.onClick.AddListener(() => AddEnemy(buttonAdd, 1));
            button5Add.onClick.AddListener(() => AddEnemy(button5Add, 5));
            buttonSub.onClick.AddListener(() => SubEnemy(buttonSub, 1));
            button5Sub.onClick.AddListener(() => SubEnemy(button5Sub, 5));

            InitializetextCharacters();
        }

        private void InitializetextCharacters()
        {
            textCharacter.GetComponent<TMPro.TextMeshProUGUI>().color = colorCharacterText;
            textCharacter.GetComponent<TMPro.TextMeshProUGUI>().text =
                "\n  Characters: \n" +
                " Health: " + enemy.StartHealth.ToString() + "\n" +
                " Speed: " + enemy.StartSpeed.ToString() + "\n" +
                " Damage: " + enemy.Damage.ToString();
        }

        public void ChangeImageToCharacter()
        {
            if (characterEnemy.activeSelf)
            {
                characterEnemy.SetActive(false);
                textCharacter.SetActive(false);
            }
            else
            {
                characterEnemy.SetActive(true);
                textCharacter.SetActive(true);
            }

        }

        public void ClearCount()
        {
            count = 0;
            textCount.text = count.ToString();
        }

        private void AddEnemy(Button clickedButton, int countAdd)
        {
            if (enemyShopMenu.AddEnemy(enemy.EnemyType, enemy.Price, countAdd))
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

        private void SubEnemy(Button clickedButton, int countSub)
        {

            if (enemyShopMenu.SubEnemy(enemy.EnemyType, enemy.Price, count, countSub))
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
