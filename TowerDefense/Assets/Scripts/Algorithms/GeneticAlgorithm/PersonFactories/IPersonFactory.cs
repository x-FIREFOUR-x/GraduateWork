using System.Collections.Generic;

using TowerDefense.Algorithms.GeneticAlgorithm.Persons;


namespace TowerDefense.Algorithms.GeneticAlgorithm.PersonFactories
{
    public interface IPersonFactory <TPerson, TGene> where TPerson : Person<TGene>
    {
        public TPerson CreatePerson();

        public TPerson CreatePerson(List<TGene> genes, int price, int value);

        public TPerson CreatePerson(List<TGene> genes);
    }
}
