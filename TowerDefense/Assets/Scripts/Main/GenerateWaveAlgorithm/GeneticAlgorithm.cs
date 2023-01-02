using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm
{
    int countPerson = 20;
    int countIteration = 1000;

    int mutationChance = 25;
    int maxMutationPartPerson = 5;

    List<Person> currentPopulation;
  
    private int totalPrice;
    private List<EnemyType> _enemysTypes;
    private Dictionary<EnemyType, int> _enemysPrices;

    [SerializeField]
    private HeuristicsCalculator heuristics;



    public GeneticAlgorithm()
    {
        heuristics = new();
    }

    public List<EnemyType> SearchKnapsac(List<EnemyType> enemysTypes, Dictionary<EnemyType, int> enemysPrices)
    {
        currentPopulation = new();

        _enemysTypes = enemysTypes;
        _enemysPrices = enemysPrices;
        totalPrice = heuristics.TotalPlayerMoney();

        CreateStartPopulation();
        SortPopulation();
        for (int i = 0; i < countIteration; i++)
        {
            CreateNewPopulation();
            SortPopulation();
        }

        return currentPopulation[0].Enemies;
    }


    private void SortPopulation()
    {
        currentPopulation.Sort((a, b) => b.Value.CompareTo(a.Value));
    }

    private void CreateStartPopulation()
    {
        for (int i = 0; i < countPerson; i++)
        {
            (List<EnemyType> newPerson, int price) = CreateListEnemiesForPrice(totalPrice);
            currentPopulation.Add(new Person(newPerson, price, heuristics.GetHeuristicsValue(newPerson)));
        }
    }

    private void CreateNewPopulation()
    {
        Person newBestPerson = new();

        for (int i = 0; i < currentPopulation.Count - 1; i++)
        {
            Person newPerson = Crossing(currentPopulation[i], currentPopulation[i + 1]);

            if (newPerson.Price <= totalPrice)
            {
                newPerson.CalculateValue(heuristics);

                if (newPerson.Value > newBestPerson.Value)
                {
                    newBestPerson = newPerson;
                }
            }
        }
        int rand = Random.Range(0, 100);
        if(rand < mutationChance)
            newBestPerson = MutationRandomRegenerateRange(newBestPerson);
        if (rand >= mutationChance && rand < 2 * mutationChance && _enemysTypes.Count > 1)
            newBestPerson = MutationChangeRangeOneType(newBestPerson);

        if (newBestPerson.Value > currentPopulation[currentPopulation.Count -1].Value)
        {
            currentPopulation[currentPopulation.Count - 1] = newBestPerson;
        }
    }


    private Person Crossing(Person person1, Person person2)
    {
        Person newPerson = new();

        int index1Mid = (int)person1.Enemies.Count / 2;
        int index2Mid = (int)person2.Enemies.Count / 2;

        newPerson.Enemies = new(person1.Enemies.GetRange(0, index1Mid));
        newPerson.CalculatePrice(_enemysPrices);

        //for (int i = index2Mid; i < person2.Enemies.Count; i++)
        for (int i = person2.Enemies.Count - 1; i >= 0; i--)
        {
            if (newPerson.Price + _enemysPrices[person2.Enemies[i]] <= totalPrice)
            {
                newPerson.Enemies.Add(person2.Enemies[i]);
                newPerson.Price += _enemysPrices[person2.Enemies[i]];
            }
        }

        return newPerson;
    }


        //Mutation: randomly regenerate a random interval
    private Person MutationRandomRegenerateRange(Person person)
    {
        int index = Random.Range(0, person.Enemies.Count);

        int maxSize = (int)(person.Enemies.Count / maxMutationPartPerson);
        maxSize = (maxSize == 0) ? 1 : maxSize;
        maxSize = (maxSize > person.Enemies.Count - index) ? person.Enemies.Count - index : maxSize;
        int size = Random.Range(1, maxSize + 1);

        int priceOldPart = person.GetPriceRange(index, size, _enemysPrices);
        (List<EnemyType> newPart, int priceNewPart) = CreateListEnemiesForPrice(priceOldPart + totalPrice - person.Price);

        Person newPerson = new Person(new List<EnemyType>(person.Enemies));
        newPerson.Enemies.RemoveRange(index, size);
        newPerson.Enemies.InsertRange(index, newPart);
        newPerson.Price = person.Price - priceOldPart + priceNewPart;

        newPerson.CalculateValue(heuristics);
        if (newPerson.Value > person.Value)
            return newPerson;
        else
            return person;
    }

    //Mutation: change random interval to one type enemies
    private Person MutationChangeRangeOneType(Person person)
    {
        int index = Random.Range(0, person.Enemies.Count);

        int maxSize = (int)(person.Enemies.Count / maxMutationPartPerson);
        maxSize = (maxSize == 0) ? 1 : maxSize;
        maxSize = (maxSize > person.Enemies.Count - index) ? person.Enemies.Count - index : maxSize;
        int size = Random.Range(1, maxSize + 1);

        EnemyType type = _enemysTypes[Random.Range(1, _enemysTypes.Count)];

        int priceOldPart = person.GetPriceRange(index, size, _enemysPrices);
    
        (List<EnemyType> newPart, int priceNewPart) = CreateListOneTypeEnemiesForPrice(priceOldPart, type);
        (List<EnemyType> additionalPart, int priceAdditionalPart) = CreateListOneTypeEnemiesForPrice(priceOldPart - priceNewPart, _enemysTypes[0]);

        priceNewPart += priceAdditionalPart;
        newPart.AddRange(additionalPart);

        Person newPerson = new Person(new List<EnemyType>(person.Enemies));
        newPerson.Enemies.RemoveRange(index, size);
        newPerson.Enemies.InsertRange(index, newPart);
        newPerson.Price = person.Price - priceOldPart + priceNewPart;

        newPerson.CalculateValue(heuristics);
        if (newPerson.Value > person.Value)
            return newPerson;
        else
            return person;
    }




    private (List<EnemyType>, int) CreateListEnemiesForPrice(int sumPrice)
    {
        int price = 0;
        List<EnemyType> newPerson = new();

        bool isFinish = false;
        while (!isFinish)
        {
            int indexRandomEnemy = Random.Range(0, _enemysTypes.Count);

            if (price + _enemysPrices[_enemysTypes[indexRandomEnemy]] <= sumPrice)
            {
                newPerson.Add(_enemysTypes[indexRandomEnemy]);
                price += _enemysPrices[_enemysTypes[indexRandomEnemy]];
            }
            else
            {
                isFinish = true;
            }
        }

        return (newPerson, price);
    }

    private (List<EnemyType>, int) CreateListOneTypeEnemiesForPrice(int sumPrice, EnemyType type)
    {
        int price = 0;
        List<EnemyType> newPerson = new();

        bool isFinish = false;
        while (!isFinish)
        {
            if (price + _enemysPrices[type] <= sumPrice)
            {
                newPerson.Add(type);
                price += _enemysPrices[type];
            }
            else
            {
                isFinish = true;
            }
        }

        return (newPerson, price);
    }
}