using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repairs.Composite
{
    // Roulette repair: on every call one of the repairs is chosen by roulette wheel
    // with probabilities that never change, and the individual is repaired by it; the rest of the wheel means no repair,
    // and then the individual itself is returned unchanged.
    // Mixes repairs of different kinds in one search, for example mostly greedy repair to keep good genes
    // and sometimes random repair to keep the diversity of the population.
    // With one repair and probability p it repairs only a share p of the individuals and keeps the rest infeasible,
    // close to the border of the feasible region, where good solutions often lie; repairing only a part of them
    // is suggested by Orvosh and Davis (1993).
    // The probabilities are absolute: each is in [0; 1] and their sum is at most 1; NoRepairProbability is 1 minus their sum.
    // Without probabilities every repair has the same probability and some repair is always applied.
    // An individual left unrepaired breaks the constraint, so with NoRepairProbability above 0 the fitness function
    // or the comparer of individuals must account for it, for example by a penalty.
    // It requires the random number generator, so calling it with null throws; the generator is passed to the chosen repair.
    //
    // Example: repairs [GreedyRepair, RandomRepair], probabilities [0.8, 0.2] -> greedy repair in 80% of calls, random in 20%;
    // repairs [GreedyRepair], probabilities [0.2] -> greedy repair in 20% of calls, no repair in 80%.
    //
    // Complexity: time O(m) plus the time of the chosen repair
    //             memory O(m)
    // m - the number of repairs.
    public sealed class RouletteRepair<TGene> : IRepair<TGene>
    {
        private readonly double _sumRepairsProbabilities;

        public bool PreservesIndividualLength => Repairs.All(repair => repair.PreservesIndividualLength);

        // Repairs to choose from; at least one, none of them null.
        public IReadOnlyList<IRepair<TGene>> Repairs { get; }

        // Probability of choosing each repair, in the order of the repairs; each in [0; 1], their sum at most 1.
        public IReadOnlyList<double> Probabilities { get; }

        // Probability that no repair is applied and the individual is returned unchanged: 1 minus the sum of Probabilities.
        public double NoRepairProbability { get; }


        public RouletteRepair(IReadOnlyList<IRepair<TGene>> repairs, IReadOnlyList<double> probabilities = null)
        {
            RepairUtilities.ValidateRepairs(repairs, nameof(repairs));
            Repairs = repairs.ToArray();
            Probabilities = ProbabilityUtilities.CreateAbsoluteProbabilities(probabilities, Repairs.Count, nameof(probabilities));

            _sumRepairsProbabilities = ProbabilityUtilities.SumProbabilities(Probabilities);
            NoRepairProbability = 1 - _sumRepairsProbabilities;
        }

        public Individual<TGene> Repair(Individual<TGene> individual, IRandom random = null)
        {
            RepairUtilities.ValidateIndividualNotNull(individual);
            RepairUtilities.ValidateRandomNotNull(random);

            double roll = random.NextDouble();
            // The roll is beyond the segments of all repairs, so no repair is applied.
            if (roll >= _sumRepairsProbabilities)
                return individual;

            int repairIndex = ProbabilityUtilities.ChooseIndexByProbability(Probabilities, roll);
            return Repairs[repairIndex].Repair(individual, random);
        }
    }
}
