using System.Collections.Generic;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Persons
{
    public class PersonFactory<TGene>
    {
        public Person<TGene> CreatePerson()
        {
            return new Person<TGene>();
        }

        public Person<TGene> CreatePerson(List<TGene> genes, int price, float value)
        {
            return new Person<TGene>(genes, price, value);
        }

        public Person<TGene> CreatePerson(List<TGene> genes)
        {
            return new Person<TGene>(genes);
        }
    }
}
