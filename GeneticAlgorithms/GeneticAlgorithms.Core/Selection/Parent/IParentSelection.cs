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

        // Whether the selection prefers individuals by reading their fitness values directly, as fitness-proportional selections do,
        // so it considers the constraint only if the fitness values do; false for selections that compare or pick at random.
        bool UsesFitnessValues => false;

        // parentPairCount: number of parent pairs, one per offspring.
        IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            int parentPairCount,
            IRandom random);
    }
}
