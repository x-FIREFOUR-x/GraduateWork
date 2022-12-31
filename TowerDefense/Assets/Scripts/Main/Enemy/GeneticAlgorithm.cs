using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm
{
    int countPerson = 10;
    int countIteration = 100;
    float mutationChance = 10;

    List<(List<EnemyType>, int)> currentPopulation;
  
    private float totalPrice;
    private List<EnemyType> _enemysTypes;
    private List<int> _enemysPrices;

    [SerializeField]
    private HeuristicsCalculator heuristics;

    public GeneticAlgorithm()
    {
        heuristics = new();
    }

    public List<EnemyType> SearchKnapsac(List<EnemyType> enemysTypes, List<int> enemysPrices)
    {
        currentPopulation = new();

        _enemysTypes = enemysTypes;
        _enemysPrices = enemysPrices;
        totalPrice = heuristics.TotalPlayerMoney();

        CreateStartPopulation();
        for (int i = 0; i < countIteration; i++)
        {
            SortPopulation();
            CreateNewPopulation();
        }

        return currentPopulation[0].Item1;
    }

    private void CreateStartPopulation()
    {
        for (int i = 0; i < countPerson; i++)
        {
            int price = 0;
            List<EnemyType> newPerson = new();

            bool isFinish = false; 

            while (!isFinish)
            {
                int indexRandomEnemy = Random.Range(0, _enemysTypes.Count);

                if (price + _enemysPrices[indexRandomEnemy] <= totalPrice)
                {
                    newPerson.Add(_enemysTypes[indexRandomEnemy]);
                    price += _enemysPrices[indexRandomEnemy];
                }
                else
                {
                    isFinish = true;
                }
            }

            //currentPopulation.Add((newPerson, heuristics.GetHeuristicsValue(newPerson)));
            currentPopulation.Add((newPerson, price));
        }
    }

    private void SortPopulation()
    {
        currentPopulation.Sort((a, b) => b.Item2.CompareTo(a.Item2));
    }

    private void CreateNewPopulation()
    {

    }
}
