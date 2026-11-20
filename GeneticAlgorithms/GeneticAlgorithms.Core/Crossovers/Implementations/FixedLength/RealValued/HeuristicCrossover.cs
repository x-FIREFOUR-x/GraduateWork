using System;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Crossovers.Implementations.FixedLength.RealValued
{
    // Heuristic crossover (Wright, 1991; bounds handling as described by Michalewicz, 1992): the only crossover here
    // that uses fitness. A random weight r is drawn from [0; 1), and child = better + r * (better - worse): the child steps
    // from the worse parent through the better one and beyond it, in the direction in which fitness grew.
    // If some child gene falls outside [MinGeneValue; MaxGeneValue], a new r is drawn, up to MaxAttemptCount times;
    // if no attempt succeeds, the child is a copy of the better parent. Both parents must be evaluated
    // and their genes must lie within the bounds, so the child always lies within the bounds too.
    //
    // Complexity: time O(w * n)
    //             memory O(n)
    // n - the number of genes of a parent;
    // w - the max attempt count.
    public sealed class HeuristicCrossover : ICrossover<double>
    {
        private readonly IConstraint<double> _constraint;

        // Smallest allowed gene value; a child with a smaller gene is drawn again.
        public double MinGeneValue { get; }

        // Largest allowed gene value; a child with a larger gene is drawn again.
        public double MaxGeneValue { get; }

        // How many weights r are tried before the better parent is copied instead; at least 1.
        public int MaxAttemptCount { get; }

        public IIndividualComparer<double> Comparer { get; }


        public HeuristicCrossover(
            IConstraint<double> constraint,
            double minGeneValue = double.NegativeInfinity,
            double maxGeneValue = double.PositiveInfinity,
            int maxAttemptCount = 10,
            IIndividualComparer<double> comparer = null)
        {
            CrossoverUtilities.ValidateGeneBounds(minGeneValue, maxGeneValue);
            if (maxAttemptCount < 1)
                throw new ArgumentOutOfRangeException(nameof(maxAttemptCount), maxAttemptCount, "Max attempt count must be at least 1.");

            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            MinGeneValue = minGeneValue;
            MaxGeneValue = maxGeneValue;
            MaxAttemptCount = maxAttemptCount;
            Comparer = comparer ?? new FitnessComparer<double>();
        }

        public Individual<double> Cross(Individual<double> firstParent, Individual<double> secondParent, IRandom random)
        {
            CrossoverUtilities.ValidateArgumentsNotNull(firstParent, secondParent, random);
            CrossoverUtilities.ValidateParentsHaveEqualLength(firstParent, secondParent);
            CrossoverUtilities.ValidateFiniteGenes(firstParent, secondParent);
            CrossoverUtilities.ValidateGenesWithinBounds(firstParent, secondParent, MinGeneValue, MaxGeneValue);

            var (betterParent, worseParent) = Comparer.IsWorse(firstParent, secondParent) ? (secondParent, firstParent) : (firstParent, secondParent);

            var childGenes = new double[betterParent.Count];
            for (int attempt = 0; attempt < MaxAttemptCount; attempt++)
            {
                double weight = random.NextDouble();
                bool isWithinBounds = true;
                for (int i = 0; i < childGenes.Length && isWithinBounds; i++)
                {
                    childGenes[i] = betterParent.Genes[i] + weight * (betterParent.Genes[i] - worseParent.Genes[i]);
                    isWithinBounds = childGenes[i] >= MinGeneValue && childGenes[i] <= MaxGeneValue;
                }

                if (isWithinBounds)
                    return Individual<double>.FromGenes(_constraint, childGenes);
            }

            return betterParent.Clone();
        }
    }
}
