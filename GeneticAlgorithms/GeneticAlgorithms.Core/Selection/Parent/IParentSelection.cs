using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent
{
    // Parent selection: selects the requested number of parent pairs for one generation.
    // Each pair is crossed over to produce one offspring.
    public interface IParentSelection<TGene>
    {
        // Decides which individual is better; null when the selection does not compare individuals:
        // random selection, or a fitness-proportional selection that reads fitness values directly (see UsesFitnessValues).
        IIndividualComparer<TGene> Comparer { get; }

        // Whether the selector prefers individuals by reading their fitness values directly, as fitness-proportional selections do,
        // so it prefers feasible individuals only if the fitness values do; false for selections that compare or pick at random.
        bool UsesFitnessValues { get; }

        // Whether the selector needs a comparer that orders every pair of individuals, such as FitnessComparer.
        // true: it sorts individuals or reads a single fitness value, so it cannot be used with Pareto dominance.
        // false: it only compares pairs or picks at random, so it runs correctly with Pareto dominance.
        // false does not mean the selection is useful for several objectives: that is shown by its folder (SingleObjective, Common or MultiObjective).
        bool RequiresTotalOrder { get; }

        // parentPairCount: number of parent pairs, one per offspring.
        IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            int parentPairCount,
            IRandom random);
    }
}
