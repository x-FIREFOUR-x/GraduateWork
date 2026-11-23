using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;

namespace GeneticAlgorithms.Selection.Parent.Implementations.SingleObjective.Rank
{
    public sealed class AdjacentPairsParentSelection<TGene> : IParentSelection<TGene>
    {
        public IIndividualComparer<TGene> Comparer { get; }

        public bool UsesFitnessValues => false;

        public bool RequiresTotalOrder => true;


        public AdjacentPairsParentSelection(IIndividualComparer<TGene> comparer = null)
        {
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, parentPairCount, random);

            var rankedPopulation = ParentSelectionUtilities.SortWorstToBest(population, Comparer);
            Array.Reverse(rankedPopulation);

            int adjacentPairCount = Math.Max(1, rankedPopulation.Length - 1);
            var pairs = new ParentPair<TGene>[parentPairCount];
            for (int i = 0; i < parentPairCount; i++)
            {
                int firstIndex = i % adjacentPairCount;
                int secondIndex = Math.Min(firstIndex + 1, rankedPopulation.Length - 1);
                pairs[i] = new ParentPair<TGene>(rankedPopulation[firstIndex], rankedPopulation[secondIndex]);
            }
            return pairs;
        }
    }
}
