using System.Collections.Generic;

using TowerDefense.Main.Enemies;
using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Persons
{
    public class EnemyPerson : Person<EnemyType>
    {
        public EnemyPerson()
        {
            Genes = new();
            Price = 0;
            Value = 0;
        }

        public EnemyPerson(List<EnemyType> enemies, int price, int value)
        {
            Genes = enemies;
            Price = price;
            Value = value;
        }

        public EnemyPerson(List<EnemyType> enemies)
        {
            Genes = enemies;
            Price = 0;
            Value = 0;
        }

        public override void CalculatePrice(Dictionary<EnemyType, int> _enemysPrices) 
        {
            Price = 0;
            for (int i = 0; i < Genes.Count; i++)
            {
                Price += _enemysPrices[Genes[i]];
            }
        }

        public override void CalculateValue()
        {
            Value = EnemiesHeuristicsCalculator.instance.GetHeuristicsValue(Genes);
        }

        public override int GetPriceRangeGenes(int index, int size, Dictionary<EnemyType, int> _enemysPrices)
        {
            int priceRange = 0;
            for (int i = index; i < size; i++)
            {
                priceRange += _enemysPrices[Genes[i]];
            }
            return priceRange;
        }
    }

}
