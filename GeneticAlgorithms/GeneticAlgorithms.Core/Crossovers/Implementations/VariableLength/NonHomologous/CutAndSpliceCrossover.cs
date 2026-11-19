using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.VariableLength.NonHomologous
{
    // Cut-and-splice crossover: each parent is cut at its own CutPointCount random points, and the child takes the segments
    // alternately from the first and the second parent. Since the cut points of the parents are independent, the child may be
    // shorter or longer than both parents. CutPointCount = 1 is the cut-and-splice of messy GA (Goldberg, Korb, Deb, 1989):
    // a prefix of the first parent and a suffix of the second; CutPointCount = 2 is the crossover of SAGA (Harvey, 1992):
    // a segment of the first parent is replaced by a segment of the second.
    //
    // Complexity: time O(n + m + k * log k)
    //             memory O(n + m + k)
    // n - the number of genes of the first parent;
    // m - the number of genes of the second parent;
    // k - the cut point count.
    public sealed class CutAndSpliceCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        // How many cut points split each parent; at least 1.
        public int CutPointCount { get; }


        public CutAndSpliceCrossover(IConstraint<TGene> constraint, int cutPointCount = 1)
        {
            if (cutPointCount < 1)
                throw new ArgumentOutOfRangeException(nameof(cutPointCount), cutPointCount, "Cut point count must be at least 1.");
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            CutPointCount = cutPointCount;
        }

        public Individual<TGene> Cross(Individual<TGene> firstParent, Individual<TGene> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);

            var firstCutPoints = CrossoverUtilities.CreateRandomCutPoints(firstParent.Count, CutPointCount, random);
            var secondCutPoints = CrossoverUtilities.CreateRandomCutPoints(secondParent.Count, CutPointCount, random);

            var child = new Individual<TGene>(_constraint.CreateTracker());
            CrossoverUtilities.AppendSegmentsAlternatingParents(child, firstParent, secondParent, firstCutPoints, secondCutPoints);
            return child;
        }
    }
}
