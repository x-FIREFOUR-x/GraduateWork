using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.SingleObjective.Rank
{
    // Truncation selection: a parent is drawn uniformly from the TruncationShare best individuals
    // of the population (at least one). Very strong selection pressure, risk of premature convergence.
    //
    // Complexity: time O(n * log n + m), memory O(n + m).
    // n - the population size;
    // m - the number of selected parents (2 * parentPairCount).
    public sealed class TruncationParentSelection<TGene> : IParentSelection<TGene>
    {
        public IIndividualComparer<TGene> Comparer { get; }

        public bool UsesFitnessValues => false;

        public bool RequiresTotalOrder => true;

        /// Share of the population, counted from the best, that may become parents, in (0; 1].
        public double TruncationShare { get; }


        public TruncationParentSelection(double truncationShare = 0.5, IIndividualComparer<TGene> comparer = null)
        {
            if (truncationShare <= 0 || truncationShare > 1)
                throw new ArgumentOutOfRangeException(nameof(truncationShare), truncationShare, "Truncation share must be greater than 0 and at most 1.");
            TruncationShare = truncationShare;
            Comparer = comparer ?? new FitnessComparer<TGene>();
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, parentPairCount, random);

            int parentCount = 2 * parentPairCount;
            var parents = new Individual<TGene>[parentCount];

            var rankedPopulation = ParentSelectionUtilities.SortWorstToBest(population, Comparer);
            int eliteCount = Math.Max(1, (int)Math.Round(TruncationShare * rankedPopulation.Length));
            int firstEliteIndex = rankedPopulation.Length - eliteCount;
            
            for (int i = 0; i < parentCount; i++)
                parents[i] = rankedPopulation[random.Next(firstEliteIndex, rankedPopulation.Length)];

            return ParentSelectionUtilities.ToPairs(parents);
        }
    }
}
