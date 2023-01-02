using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm
{
    int countPerson = 20;
    int countIteration = 1000;

    int mutationChance = 10;
    int maxMutationPartPerson = 10;

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

        newBestPerson = MutationRandomRegenerateRange(newBestPerson);

        if(newBestPerson.Value > currentPopulation[currentPopulation.Count -1].Value)
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
        int rand = Random.Range(0, 100);
        if (rand < mutationChance)
        {
            int index = Random.Range(0, person.Enemies.Count);
            int size = Random.Range(1, person.Enemies.Count - index + 1);

            int maxSize = (int)(person.Enemies.Count / maxMutationPartPerson);
            if (maxSize == 0)
                maxSize = 1;

            size = (size > maxSize)
                ? maxSize
                : size;

            Person mutatingPartPerson = new();
            mutatingPartPerson.Enemies = new(person.Enemies.GetRange(index, size));
            mutatingPartPerson.CalculatePrice(_enemysPrices);

            (List<EnemyType> mutatedPart, int pricemutatedPart) = CreateListEnemiesForPrice(mutatingPartPerson.Price + totalPrice - person.Price);

            Person newPerson = new();
            newPerson.Enemies = new (person.Enemies);
            newPerson.Enemies.RemoveRange(index, size);
            newPerson.Enemies.InsertRange(index, mutatedPart);
            newPerson.Price = person.Price - mutatingPartPerson.Price + pricemutatedPart;

            newPerson.CalculateValue(heuristics);

            if (newPerson.Value > person.Value)
                return newPerson;
            else
                return person;
        }

        return person;
    }
}
