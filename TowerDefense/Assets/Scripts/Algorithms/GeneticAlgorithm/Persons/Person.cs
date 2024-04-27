using System.Collections.Generic;

using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Persons
{
    public class Person<TGene>
    {
        public List<TGene> Genes { get; set; }
        public int Price { get; set; }
        public int Value { get; set; }


        public Person()
        {
            Genes = new();
            Price = 0;
            Value = 0;
        }

        public Person(List<TGene> genes, int price, int value)
        {
            Genes = genes;
            Price = price;
            Value = value;
        }

        public Person(List<TGene> genes)
        {
            Genes = genes;
            Price = 0;
            Value = 0;
        }


        public void CalculatePrice(Dictionary<TGene, int> genesPrices)
        {
            Price = 0;
            for (int i = 0; i < Genes.Count; i++)
            {
                Price += genesPrices[Genes[i]];
            }
        }
        public void CalculateValue(HeuristicsCalculator<TGene> heuristicsCalculator)
        {
            Value = heuristicsCalculator.GetHeuristicsValue(Genes);
        }

        public int GetPriceRangeGenes(int index, int size, Dictionary<TGene, int> _genesPrices)
        {
            int priceRange = 0;
            for (int i = index; i < size; i++)
            {
                priceRange += _genesPrices[Genes[i]];
            }
            return priceRange;
        }
    }
}
