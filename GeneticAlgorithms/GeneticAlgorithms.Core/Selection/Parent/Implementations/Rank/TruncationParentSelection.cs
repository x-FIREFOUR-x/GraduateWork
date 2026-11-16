using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Rank
{
    // Truncation selection: a parent is drawn uniformly from the TruncationShare best individuals
    // of the population (at least one). Very strong selection pressure, risk of premature convergence.
    public sealed class TruncationParentSelection<TGene> : IParentSelection<TGene>
    {
        public double TruncationShare { get; }


        public TruncationParentSelection(double truncationShare = 0.5)
        {
            if (truncationShare <= 0 || truncationShare > 1)
                throw new ArgumentOutOfRangeException(nameof(truncationShare), truncationShare, "Truncation share must be greater than 0 and at most 1.");
            TruncationShare = truncationShare;
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IIndividualComparer<TGene> comparer,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, comparer, parentPairCount, random);

            int parentCount = 2 * parentPairCount;
            var parents = new Individual<TGene>[parentCount];

            var rankedPopulation = ParentSelectionUtilities.SortWorstToBest(population, comparer);
            int eliteCount = Math.Max(1, (int)Math.Round(TruncationShare * rankedPopulation.Length));
            int firstEliteIndex = rankedPopulation.Length - eliteCount;
            
            for (int i = 0; i < parentCount; i++)
                parents[i] = rankedPopulation[random.Next(firstEliteIndex, rankedPopulation.Length)];

            return ParentSelectionUtilities.ToPairs(parents);
        }
    }
}
