using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.RealValued
{
    // Blend crossover, BLX-alpha (Eshelman, Schaffer, 1993): for every gene, d = |firstParent - secondParent|, and the child gene is drawn
    // uniformly from [min(firstParent, secondParent) - Alpha * d; max(firstParent, secondParent) + Alpha * d], where Alpha = ParentGeneRangeExtensionFactor.
    // With Alpha = 0 the child stays between the parents; Alpha = 0.5 is the usual choice that keeps the spread of the population stable.
    // Child genes are clamped to [MinGeneValue; MaxGeneValue].
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent.
    public sealed class BlendAlphaCrossover : ICrossover<double>
    {
        private readonly IConstraint<double> _constraint;

        // How far beyond the parents a gene may go, as a share of the distance between the parent genes; non-negative.
        public double ParentGeneRangeExtensionFactor { get; }

        // Smallest allowed gene value; smaller child genes are raised to it.
        public double MinGeneValue { get; }

        // Largest allowed gene value; larger child genes are lowered to it.
        public double MaxGeneValue { get; }


        public BlendAlphaCrossover(
            IConstraint<double> constraint,
            double parentGeneRangeExtensionFactor = 0.5,
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
                double smaller = Math.Min(firstParent.Genes[i], secondParent.Genes[i]);
                double larger = Math.Max(firstParent.Genes[i], secondParent.Genes[i]);
                double geneRangeExtension = ParentGeneRangeExtensionFactor * (larger - smaller);
                double gene = smaller - geneRangeExtension + random.NextDouble() * (larger - smaller + 2 * geneRangeExtension);

                child.Append(Math.Clamp(gene, MinGeneValue, MaxGeneValue));
            }

            return child;
        }
    }
}
