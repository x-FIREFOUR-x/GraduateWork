using System;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repair.Composite
{
    // Probabilistic repair: applies the nested repair with probability Probability; otherwise the individual itself is returned unchanged.
    // Keeps some infeasible individuals in the population, close to the border of the feasible region,
    // where good solutions often lie; repairing only a part of them is suggested by Orvosh and Davis (1993).
    // An individual left unrepaired breaks the constraint, so the fitness function or the comparer of individuals
    // must account for it, for example by a penalty.
    // It requires the random number generator, so calling it with null throws; the generator is passed to the nested repair.
    //
    // Example: Probability 0.2 -> on average 2 of every 10 individuals are repaired.
    //
    // Complexity: time O(1) plus the time of the nested repair when it is applied
    //             memory O(1)
    public sealed class ProbabilisticRepair<TGene> : IRepair<TGene>
    {
        private readonly IRepair<TGene> _repair;

        // Probability that the nested repair is applied, in [0; 1]; 0 never repairs and 1 always does.
        public double Probability { get; }


        public ProbabilisticRepair(IRepair<TGene> repair, double probability)
        {
            _repair = repair ?? throw new ArgumentNullException(nameof(repair));

            if (!(probability >= 0 && probability <= 1))
                throw new ArgumentOutOfRangeException(nameof(probability), probability, "Probability must be between 0 and 1.");

            Probability = probability;
        }

        public Individual<TGene> Repair(Individual<TGene> individual, IRandom random = null)
        {
            RepairUtilities.ValidateIndividualNotNull(individual);
            RepairUtilities.ValidateRandomNotNull(random);

            if (random.NextDouble() < Probability)
                return _repair.Repair(individual, random);

            return individual;
        }
    }
}
