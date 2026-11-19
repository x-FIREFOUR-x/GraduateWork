using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.Discrete
{
    // K-point crossover (De Jong, 1975): CutPointCount distinct cut points split both parents at the same positions,
    // and the child takes the segments between them alternately from the first and the second parent.
    // One-point and two-point crossover are the cases CutPointCount = 1 and CutPointCount = 2.
    // If the parents are too short, the number of cut points is reduced to the number of genes minus 1.
    //
    // Complexity: time O(n + k * log k)
    //             memory O(n + k)
    // n - the number of genes of a parent;
    // k - the cut point count.
    public sealed class KPointCrossover<TGene> : ICrossover<TGene>
    {
        private readonly IConstraint<TGene> _constraint;

        // How many cut points split the parents; at least 1.
        public int CutPointCount { get; }


        public KPointCrossover(IConstraint<TGene> constraint, int cutPointCount = 1)
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

            var cutPoints = CrossoverUtilities.CreateRandomDistinctCutPoints(firstParent.Count, CutPointCount, random);
            var child = new Individual<TGene>(_constraint.CreateTracker());
            CrossoverUtilities.AppendSegmentsAlternatingParents(child, firstParent, secondParent, cutPoints, cutPoints);
            return child;
        }
    }
}
