using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Rank
{
    // Exponential ranking selection: individuals are ordered from best to worst, and the weight of the individual
    // at position i (0 is the best) equals c^i, where c = RankBase in (0; 1]. The smaller c, the stronger
    // the selection pressure; c = 1 gives uniform selection. Independent of the fitness scale; uses comparisons only.
    //
    // Complexity: time O((n + m) * log n), memory O(n + m).
    // n - the population size;
    // m - the number of selected parents (2 * parentPairCount).
    public sealed class ExponentialRankParentSelection<TGene> : IParentSelection<TGene>
    {
        public double RankBase { get; }


        public ExponentialRankParentSelection(double rankBase = 0.9)
        {
            if (rankBase <= 0 || rankBase > 1)
                throw new ArgumentOutOfRangeException(nameof(rankBase), rankBase, "Rank base must be greater than 0 and at less 1.");
            RankBase = rankBase;
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IIndividualComparer<TGene> comparer,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, comparer, parentPairCount, random);
            int parentCount = 2 * parentPairCount;

            var rankedPopulation = ParentSelectionUtilities.SortWorstToBest(population, comparer);

            var selectionProbabilityWeights = new double[rankedPopulation.Length];
            double probabilityWeight = 1;
            for (int rank = rankedPopulation.Length - 1; rank >= 0; rank--)
            {
                selectionProbabilityWeights[rank] = probabilityWeight;
                probabilityWeight *= RankBase;
            }

            return ParentSelectionUtilities.ToPairs(ParentSelectionUtilities.SelectParentsByProbabilityWeights(rankedPopulation, selectionProbabilityWeights, parentCount, random));
        }
    }
}
