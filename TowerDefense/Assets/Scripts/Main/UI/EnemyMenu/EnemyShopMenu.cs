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

    public bool AddEnemy(EnemyType enemy, int price)
    {
        if (price <= PlayerStats.Money)
        {
            enemies.Add(enemy);
            PlayerStats.Money -= price;

            return true;
        }

        return false;
    }

    public bool SubEnemy(EnemyType enemy, int price)
    {
        //TO DO: start look for idx from the end of the list to remove the enemy
        if (enemies.Remove(enemy))
        {
            PlayerStats.Money += price;
            return true;
        }

        return false;
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
