using System.Collections.Generic;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Persons
{
    public abstract class Person<TGene>
    {
        public List<TGene> Genes { get; set; }
        public int Price { get; set; }
        public int Value { get; set; }

        public abstract void CalculatePrice(Dictionary<TGene, int> genesPrices);
        public abstract void CalculateValue();

        public abstract int GetPriceRangeGenes(int index, int size, Dictionary<TGene, int> _genesPrices);
    }
}
