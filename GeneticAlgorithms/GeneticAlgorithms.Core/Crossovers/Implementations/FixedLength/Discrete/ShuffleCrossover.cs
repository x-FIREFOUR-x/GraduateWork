using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Discrete
{
    // Shuffle crossover (Eshelman, Caruana, Schaffer, 1989): the gene positions are shuffled, k-point crossover is applied
    // to the parents in this shuffled order, and the genes are put back to their positions. So which genes come together
    // from one parent no longer depends on how close they are in the genome, which removes the positional bias of k-point crossover.
    //
    // Complexity: time O(n + k * log k)
    //             memory O(n + k)
    // n - the number of genes of a parent;
    // k - the cut point count.
    public sealed class ShuffleCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        // How many cut points split the shuffled parents; at least 1.
        public int CutPointCount { get; }


        public ShuffleCrossover(IConstraint<TGene> constraint, int cutPointCount = 1)
        {
            if (cutPointCount < 1)
                throw new ArgumentOutOfRangeException(nameof(cutPointCount), cutPointCount, "Cut point count must be at least 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            CutPointCount = cutPointCount;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);

            var shuffledPositionsIndices = CrossoverUtilities.CreateSequentialIndices(firstParent.Count);
            CrossoverUtilities.Shuffle(shuffledPositionsIndices, random);
            var cutPointsIndeces = CrossoverUtilities.CreateRandomDistinctCutPoints(firstParent.Count, CutPointCount, random);

            var takeFromSecondParentAtPosition = new bool[firstParent.Count];
                // Select half of segments in secondParent (every second segment)
            for (int segmentNumber = 1; segmentNumber <= cutPointsIndeces.Length; segmentNumber += 2)
            {
                int segmentStart = cutPointsIndeces[segmentNumber - 1];
                int segmentEnd = segmentNumber < cutPointsIndeces.Length ? cutPointsIndeces[segmentNumber] : shuffledPositionsIndices.Length;
                for (int i = segmentStart; i < segmentEnd; i++)
                    takeFromSecondParentAtPosition[shuffledPositionsIndices[i]] = true;
            }

            var child = new Individual<TGene>(_constraint.CreateTracker());
            for (int position = 0; position < firstParent.Count; position++)
                child.Append(takeFromSecondParentAtPosition[position] ? secondParent.Genes[position] : firstParent.Genes[position]);
            return child;
        }
    }
}
