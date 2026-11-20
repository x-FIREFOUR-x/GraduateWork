using System;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Mutations.Implementations.LengthPreserving.RealValued
{
    // Non-uniform mutation (Michalewicz, 1992): the step shrinks as the search goes on,
    // so the mutation explores the whole range at the start and refines solutions at the end.
    //
    // For every chosen gene x at iteration t:
    // 1. A direction is chosen at random: towards MaxGeneValue with y = MaxGeneValue - x,
    //    or towards MinGeneValue with y = x - MinGeneValue.
    // 2. The step is delta(t, y) = y * (1 - r^((1 - t / T)^b)), with r uniform in [0; 1).
    // 3. The mutated gene is x + delta towards the max, or x - delta towards the min.
    // y - boundDistance; the distance from the gene to the bound it moves to (boundDistance);
    // T - MaxIterations; t / T is clamped to 1, so for b > 0 steps become zero after T iterations;
    // b - DependencyDegree, how fast the step shrinks with t: 0 keeps it uniform in (0; y] at every iteration,
    //     large values shrink it early.
    // The step never exceeds y, so mutated genes never leave the bounds.
    //
    // The current iteration t is the currentIterationNumber argument of Mutate, which the genetic algorithm passes on every call;
    // the mutation cannot work without it, so calling Mutate with a null currentIterationNumber throws.
    // Genes of the individual must lie within the bounds. If no gene changes, the individual itself is returned.
    //
    // Complexity: time O(n)
    //             memory O(n)
    // n - the number of genes of the individual.
    public sealed class NonUniformMutation : IMutation<double>
    {
        // Smallest allowed gene value; finite, steps towards it never go past it.
        public double MinGeneValue { get; }

        // Largest allowed gene value; finite, steps towards it never go past it.
        public double MaxGeneValue { get; }
        // Probability that a gene is mutated, in (0; 1]; null means 1 / n, so on average one gene per individual.
        public double? GeneMutationProbability { get; }

        // Iteration at which steps become zero when DependencyDegree is positive; usually the iteration limit of the stop criterion; at least 1.
        public int MaxIterations { get; }

        // How fast steps shrink with the iterations; non-negative, Michalewicz suggests 5.
        public double DependencyDegree { get; }


        public NonUniformMutation(
            double minGeneValue,
            double maxGeneValue,
            int maxIterations,
            double dependencyDegree = 5,
            double? geneMutationProbability = null)
        {
            MutationUtilities.ValidateFiniteGeneBounds(minGeneValue, maxGeneValue);
            MinGeneValue = minGeneValue;
            MaxGeneValue = maxGeneValue;

            if (maxIterations < 1)
                throw new ArgumentOutOfRangeException(nameof(maxIterations), maxIterations, "Max iterations must be at least 1.");
            MaxIterations = maxIterations;

            if (!(dependencyDegree >= 0) || double.IsInfinity(dependencyDegree))
                throw new ArgumentOutOfRangeException(nameof(dependencyDegree), dependencyDegree, "Dependency degree must be non-negative and finite.");
            DependencyDegree = dependencyDegree;

            MutationUtilities.ValidateGeneMutationProbability(geneMutationProbability);
            GeneMutationProbability = geneMutationProbability;
        }

        public Individual<double> Mutate(Individual<double> individual, IRandom random, int? currentIterationNumber = null)
        {
            MutationUtilities.ValidateArgumentsNotNull(individual, random);
            int iterationNumber = ValidateCurrentIterationNumber(currentIterationNumber);
            MutationUtilities.ValidateGenesWithinBounds(individual, MinGeneValue, MaxGeneValue);

            double iterationsShare = Math.Min(1.0, (double)iterationNumber / MaxIterations);
            double stepPower = Math.Pow(1 - iterationsShare, DependencyDegree);
            return MutationUtilities.MutateEachGene(individual, GeneMutationProbability, random, gene => MutateGene(gene, stepPower, random));
        }

        private int ValidateCurrentIterationNumber(int? currentIterationNumber)
        {
            if (!currentIterationNumber.HasValue)
                throw new ArgumentNullException(nameof(currentIterationNumber), "Non-uniform mutation requires the current iteration number.");

            if (currentIterationNumber.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(currentIterationNumber), currentIterationNumber, "Current iteration number must be non-negative.");

            return currentIterationNumber.Value;
        }

        private double MutateGene(double gene, double stepPower, IRandom random)
        {
            bool chosenTowardsMaxGeneValue = random.Next(2) == 0;
            double boundDistance = chosenTowardsMaxGeneValue ? MaxGeneValue - gene : gene - MinGeneValue;
            double delta = boundDistance * (1 - Math.Pow(random.NextDouble(), stepPower));

            double mutatedGene = chosenTowardsMaxGeneValue ? gene + delta : gene - delta;

            return Math.Clamp(mutatedGene, MinGeneValue, MaxGeneValue);
        }
    }
}
