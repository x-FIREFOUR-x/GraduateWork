using System;
using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Rank
{
    // Linear ranking selection (Baker, 1985): individuals are sorted from worst (rank 0) to best (rank n - 1),
    // and the selection probability depends linearly on the rank: p(r) = (2 - s) / n + 2r(s - 1) / (n(n - 1)),
    // where s = SelectionPressure in [1; 2] is the expected number of copies of the best individual.
    public sealed class LinearRankParentSelection<TGene> : IParentSelection<TGene>
    {
        public double SelectionPressure { get; }


        public LinearRankParentSelection(double selectionPressure = 1.5)
        {
            if (selectionPressure < 1 || selectionPressure > 2)
                throw new ArgumentOutOfRangeException(nameof(selectionPressure), selectionPressure, "Selection pressure must be between 1 and 2.");
            SelectionPressure = selectionPressure;
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IComparer<Individual<TGene>> comparer,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, comparer, parentPairCount, random);

            int parentCount = 2 * parentPairCount;

            var rankedPopulation = ParentSelectionUtilities.SortWorstToBest(population, comparer);
            int populationSize = rankedPopulation.Length;
            var selectionProbabilityWeights = new double[populationSize];

            for (int rank = 0; rank < populationSize; rank++)
            {
                selectionProbabilityWeights[rank] = populationSize == 1
                    ? 1
                        // The weights used are n * p(r) = (2 - s) + 2r(s - 1) / (n - 1); normalization happens in sampling.
                        // Independent of the fitness scale; uses comparisons only.
                    : (2 - SelectionPressure) + 2 * rank * (SelectionPressure - 1) / (populationSize - 1);
            }
            
            return ParentSelectionUtilities.ToPairs(ParentSelectionUtilities.SelectParentsByProbabilityWeights(rankedPopulation, selectionProbabilityWeights, parentCount, random));
        }
    }
}
