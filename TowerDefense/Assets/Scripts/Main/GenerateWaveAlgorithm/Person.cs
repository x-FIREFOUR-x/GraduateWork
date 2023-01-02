using System.Collections.Generic;

public class Person
{
    public List<EnemyType> Enemies { get; set; }
    public int Price { get; set; }
    public int Value { get; set; }

    public Person()
    {
        Enemies = new();
        Price = 0;
        Value = 0;
    }

    public Person(List<EnemyType> enemies, int price, int value) 
    {
        Enemies = enemies;
        Price = price;
        Value = value;
    }

    public Person(List<EnemyType> enemies)
    {
        Enemies = enemies;
        Price = 0;
        Value = 0;
    }

    public void CalculatePrice(Dictionary<EnemyType, int> _enemysPrices)
    {
        Price = 0;
        for (int i = 0; i < Enemies.Count; i++)
        {
            Price += _enemysPrices[Enemies[i]];
        }
    }

    public void CalculateValue(HeuristicsCalculator heuristics)
    {
        Value = heuristics.GetHeuristicsValue(Enemies);
    }

    public int GetPriceRange(int index, int size, Dictionary<EnemyType, int> _enemysPrices)
    {
        int priceRange = 0;
        for (int i = index; i < size; i++)
        {
            priceRange += _enemysPrices[Enemies[i]];
        }
        return priceRange;
    }
}
