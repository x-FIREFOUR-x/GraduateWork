using System.Collections.Generic;
using UnityEngine;

public class EnemyShopMenu : MonoBehaviour
{
    [SerializeField]
    private List<EnemyShopComponent> EnemyShopComponents;


    private List<EnemyType> enemies = new List<EnemyType>();

    public static string currencySymbol = "$";


    void Start()
    {
        foreach (var enemyShopComponent in EnemyShopComponents)
        {
            enemyShopComponent.Initialize();
        }
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

        foreach (var enemyShopComponent in EnemyShopComponents)
        {
            enemyShopComponent.ClearCount();
        }

        return takedEnemies;
    }
}
