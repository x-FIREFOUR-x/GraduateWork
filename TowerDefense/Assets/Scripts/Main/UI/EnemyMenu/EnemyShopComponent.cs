using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShopComponent : MonoBehaviour
{
    [SerializeField]
    private Enemy enemy;

    [SerializeField]
    private EnemyShopMenu enemyShopMenu;

    [Header("Components")]
    [SerializeField]
    private TMPro.TextMeshProUGUI textName;
    [SerializeField]
    private TMPro.TextMeshProUGUI textPrice;
    [SerializeField]
    private TMPro.TextMeshProUGUI textCount;

    [Header("Image Button Components")]
    [SerializeField]
    private GameObject characterEnemy;
    [SerializeField]
    private GameObject textCharacter;

    [Header("Colors")]
    [SerializeField]
    private Color colorText;

    private int count = 0;

    public void Initialize()
    {
        textName.color = colorText;

        textPrice.color = colorText;
        textPrice.text = enemy.Price.ToString() + "$";

        textCount.color = colorText;
        textCount.text = "0";

        InitializetextCharacters();
    }
    
    private void InitializetextCharacters()
    {
        textCharacter.GetComponent<TMPro.TextMeshProUGUI>().text =
            "   Characters: \n" +
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

    public void AddEnemy(int countAdd)
    {
        
        if (enemyShopMenu.AddEnemy(enemy.EnemyType, enemy.Price, countAdd))
        {
            count+= countAdd;
            textCount.text = count.ToString();
        }
    }

    public void SubEnemy(int countSub)
    {
        
        if(enemyShopMenu.SubEnemy(enemy.EnemyType, enemy.Price, count, countSub))
        {
            count-= countSub;
            textCount.text = count.ToString();
        }
    }

    public void ClearCount()
    {
        count = 0;
        textCount.text = count.ToString();
    }
}
