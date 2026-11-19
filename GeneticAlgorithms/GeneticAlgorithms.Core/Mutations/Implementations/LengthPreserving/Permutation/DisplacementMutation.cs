using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.Permutation
{
    // Displacement mutation (Michalewicz, 1992; see the survey of Larrañaga et al., 1999):
    // a random segment is cut out and inserted at another random position, keeping the order of genes inside it.
    // Generalises insertion mutation from one gene to a segment: keeps the set of genes and the adjacencies inside the segment.
    // 1. The segment length is chosen from 1 to n - 1, so that there is somewhere to move it.
    // 2. The segment start is chosen so that the segment fits into the individual.
    // 3. The destination is counted among the n - length + 1 places left after cutting the segment out and differs from the start.
    // Individuals with fewer than two genes are returned unchanged.
    //
    // Example: [A, B, C, D, E, F], segment [1; 3) = [B, C], destination 3 -> [A, D, E, B, C, F].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class DisplacementMutation<TGene> : IMutation<TGene>
    {
        public Individual<TGene> Mutate(Individual<TGene> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);

            if (individual.Count < 2)
                return individual;

            int segmentLength = random.Next(1, individual.Count);
            int startIndx = random.Next(individual.Count - segmentLength + 1);
            int newStartIndx = random.Next(individual.Count - segmentLength);
            if (newStartIndx >= startIndx)
                newStartIndx++;

            var mutant = individual.Clone();
            mutant.MoveRange(startIndx, segmentLength, newStartIndx);
            return mutant;
        }
    }
}
