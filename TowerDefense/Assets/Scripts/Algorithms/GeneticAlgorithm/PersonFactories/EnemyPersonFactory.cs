using System.Collections.Generic;

using TowerDefense.Main.Enemies;
using TowerDefense.Algorithms.GeneticAlgorithm.Persons;


namespace TowerDefense.Algorithms.GeneticAlgorithm.PersonFactories
{
    public class EnemyPersonFactory : IPersonFactory<EnemyPerson, EnemyType>
    {
        public EnemyPerson CreatePerson()
        {
            return new EnemyPerson();
        }

        public EnemyPerson CreatePerson(List<EnemyType> genes, int price, int value)
        {
            return new EnemyPerson(genes, price, value);
        }

        public EnemyPerson CreatePerson(List<EnemyType> genes)
        {
            return new EnemyPerson(genes);
        }
    }
}
