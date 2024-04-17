using System.Collections.Generic;
using UnityEngine;

public class EnemyShopMenu : MonoBehaviour
{
    [Header("ShopComponents")]
    [SerializeField]
    private EnemyShopComponent standardEnemyComponent;
    [SerializeField]
    private EnemyShopComponent fastEnemyComponent;
    [SerializeField]
    private EnemyShopComponent tankEnemyComponent;


    private List<EnemyType> enemies = new List<EnemyType>();

    public static string currencySymbol = "$";


    void Start()
    {
        standardEnemyComponent.Initialize();
        fastEnemyComponent.Initialize();
        tankEnemyComponent.Initialize();
    }

    public bool AddEnemy(EnemyType enemy, int price, int countAddEnemies = 1)
    {
        if (PlayerStats.GetPlayerMoney() < price * countAddEnemies)
            return false;

        for (int i = 0; i < countAddEnemies; i++)
        {
            enemies.Add(enemy);
            PlayerStats.AddPlayerMoney(-price);
        }
        
        return true;
    }

    public bool SubEnemy(EnemyType enemy, int price, int countExistEnemies, int countSubEnemies = 1)
    {
        if (countExistEnemies < countSubEnemies)
            return false;

        for (int i = 0; i < countSubEnemies; i++)
        {
            enemies.RemoveAt(enemies.LastIndexOf(enemy));
            PlayerStats.AddPlayerMoney(price);
        }

        return true;
    }

    public List<EnemyType> TakeEnemies()
    {
        List<EnemyType> takedEnemies = enemies.GetRange(0, enemies.Count);
        enemies = new List<EnemyType>();

        standardEnemyComponent.ClearCount();
        fastEnemyComponent.ClearCount();
        tankEnemyComponent.ClearCount();

        return takedEnemies;
    }
}
