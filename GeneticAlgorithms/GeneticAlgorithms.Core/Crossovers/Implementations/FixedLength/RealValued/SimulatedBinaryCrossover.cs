using System;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.RealValued
{
    // Bounded simulated binary crossover, SBX (Deb, Agrawal, 1995; bounded form as in the NSGA-II code of Deb):
    // imitates on real genes the spread of children that one-point crossover gives on binary strings.
    // Children are centred on the parents: the larger ParentProximityIndex, the closer they are to the parents.
    //
    // For every gene, with x1 <= x2 the parent genes:
    // 1. If x1 = x2, the gene is copied.
    // 2. A side is chosen at random:
    //      towards MinGeneValue with L = x1 - MinGeneValue,
    //      or towards MaxGeneValue with L = MaxGeneValue - x2.
    // 3. beta = 1 + 2L / d is the largest spread factor that keeps the child within the bound;
    //    alpha = 2 - beta^(-(eta + 1)), and alpha / 2 is the share of the unbounded distribution that lies within the bound.
    // 4. The spread factor b is drawn with u uniform in [0; 1):
    //    b = (u * alpha)^(1 / (eta + 1))            for u <= 1 / alpha (child between the parents),
    //    b = (1 / (2 - u * alpha))^(1 / (eta + 1))  otherwise (child beyond the parents).
    // 5. The child gene is (x1 + x2 - b * d) / 2 on the lower side or (x1 + x2 + b * d) / 2 on the upper side.
    // d - the distance between the parent genes, x2 - x1 (parentsGeneDistance);
    // L is the distance from the parents to that bound (boundDistance);
    // eta - ParentProximityIndex, the distribution index of the original paper;
    // b - the spread factor, |child gene - (x1 + x2) / 2| / (d / 2).
    //
    // Bounds: the part of the distribution that would fall outside the bounds is spread inside them, so children
    // never leave the bounds and do not pile up on them. Without bounds L is infinite, alpha = 2 and this is the original SBX.
    // Parent genes must lie within the bounds.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of a parent;
    public sealed class SimulatedBinaryCrossover : ICrossover<double>
    {
        private readonly IConstraint<double> _constraint;

        // How close children stay to the parents: large values give children near the parents, small values spread them; non-negative.
        public double ParentProximityIndex { get; }

        // Smallest allowed gene value; the distribution of child genes is narrowed so that they never go below it.
        public double MinGeneValue { get; }

        // Largest allowed gene value; the distribution of child genes is narrowed so that they never go above it.
        public double MaxGeneValue { get; }


        public SimulatedBinaryCrossover(
            IConstraint<double> constraint,
            double parentProximityIndex = 2,
            double minGeneValue = double.NegativeInfinity,
            double maxGeneValue = double.PositiveInfinity)
        {
            if (!(parentProximityIndex >= 0) || double.IsInfinity(parentProximityIndex))
                throw new ArgumentOutOfRangeException(nameof(parentProximityIndex), parentProximityIndex, "Parent proximity index must be non-negative and finite.");
            CrossoverUtilities.ValidateGeneBounds(minGeneValue, maxGeneValue);
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            ParentProximityIndex = parentProximityIndex;
            MinGeneValue = minGeneValue;
            MaxGeneValue = maxGeneValue;
        }

        public Individual<double> Cross(Individual<double> firstParent, Individual<double> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);
            CrossoverUtilities.ValidateFiniteGenes(firstParent, secondParent);
            CrossoverUtilities.ValidateGenesWithinBounds(firstParent, secondParent, MinGeneValue, MaxGeneValue);

            double exponent = 1 / (ParentProximityIndex + 1);
            var child = new Individual<double>(_constraint.CreateTracker());
            for (int i = 0; i < firstParent.Count; i++)
            {
                double smallerGene = Math.Min(firstParent.Genes[i], secondParent.Genes[i]);
                double largerGene = Math.Max(firstParent.Genes[i], secondParent.Genes[i]);
                double parentsGeneDistance = largerGene - smallerGene;
                if (parentsGeneDistance == 0)
                {
                    child.Append(firstParent.Genes[i]);
                    continue;
                }

                bool towardsMinGeneValue = random.Next(2) == 0;
                double boundDistance = towardsMinGeneValue ? smallerGene - MinGeneValue : MaxGeneValue - largerGene;
                double beta = 1 + 2 * boundDistance / parentsGeneDistance;
                double alpha = 2 - Math.Pow(beta, -(ParentProximityIndex + 1));

                double u = random.NextDouble();
                double spreadFactor = u <= 1 / alpha
                    ? Math.Pow(u * alpha, exponent)
                    : Math.Pow(1 / (2 - u * alpha), exponent);

                double gene = towardsMinGeneValue
                    ? 0.5 * (smallerGene + largerGene - spreadFactor * parentsGeneDistance)
                    : 0.5 * (smallerGene + largerGene + spreadFactor * parentsGeneDistance);
                child.Append(Math.Clamp(gene, MinGeneValue, MaxGeneValue));
            }
            return child;
        }
    }
}
