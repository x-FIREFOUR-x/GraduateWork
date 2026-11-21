using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.RealValued
{
    // Extended intermediate crossover (Muhlenbein, Schlierkamp-Voosen, 1993): for every gene its own random weight a is drawn
    // from [-e; 1 + e], where e = ParentGeneRangeExtensionFactor, and child = firstParent + a * (secondParent - firstParent).
    // So for x1 <= x2 the parent genes and d = x2 - x1, the child gene is uniform in [x1 - e * d; x2 + e * d]:
    // the range between the parent genes extended by e * d on each side, the same distribution as BLX-alpha.
    // With e = 0 the child lies in the box spanned by the parents; a positive e lets it go slightly beyond,
    // so the gene range does not shrink over generations.
    // Child genes are clamped to [MinGeneValue; MaxGeneValue].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class IntermediateCrossover : ICrossover<double>
    {
        private readonly IConstraint<double> _constraint;

        public bool RequiresParentsOfEqualLength => true;

        // How far beyond the parents a gene may go, as a share of the distance between the parent genes; non-negative.
        public double ParentGeneRangeExtensionFactor { get; }

        // Smallest allowed gene value; smaller child genes are raised to it.
        public double MinGeneValue { get; }

        // Largest allowed gene value; larger child genes are lowered to it.
        public double MaxGeneValue { get; }


        public IntermediateCrossover(
            IConstraint<double> constraint,
            double parentGeneRangeExtensionFactor = 0.25,
            double minGeneValue = double.NegativeInfinity,
            double maxGeneValue = double.PositiveInfinity)
        {
            if (!(parentGeneRangeExtensionFactor >= 0) || double.IsInfinity(parentGeneRangeExtensionFactor))
                throw new ArgumentOutOfRangeException(nameof(parentGeneRangeExtensionFactor), parentGeneRangeExtensionFactor, "Parent gene range extension factor must be non-negative and finite.");

            CrossoverUtilities.ValidateGeneBounds(minGeneValue, maxGeneValue);
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            ParentGeneRangeExtensionFactor = parentGeneRangeExtensionFactor;
            MinGeneValue = minGeneValue;
            MaxGeneValue = maxGeneValue;
        }

        public Individual<double> Cross(Individual<double> firstParent, Individual<double> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);
            CrossoverUtilities.ValidateFiniteGenes(firstParent, secondParent);

            var child = new Individual<double>(_constraint.CreateTracker());
            for (int i = 0; i < firstParent.Count; i++)
            {
                double weight = -ParentGeneRangeExtensionFactor + random.NextDouble() * (1 + 2 * ParentGeneRangeExtensionFactor);
                double gene = firstParent.Genes[i] + weight * (secondParent.Genes[i] - firstParent.Genes[i]);
                child.Append(Math.Clamp(gene, MinGeneValue, MaxGeneValue));
            }
            return child;
        }
    }
}
