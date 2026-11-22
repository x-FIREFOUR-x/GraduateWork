using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Repairs.Composite
{
    // Sequential repair: applies the repairs one after another, each to the result of the previous one.
    // Lets one repair be built from steps of different repairs, for example downgrading genes that break the budget
    // and then a greedy repair that fills the free limits, or a random repair and then a greedy one.
    // If no repair changes the individual, the individual itself is returned.
    // The random number generator, or null, is passed to every repair, so null works only if none of them makes random choices.
    //
    // Example: repairs [DowngradeRepair, GreedyRepair with FillRemainder], budget 10, gene cost = gene,
    // available genes [1, 3, 5, 6], genes [6, 5] -> downgrading gives [6, 3], filling spends the 1 left -> [6, 3, 1].
    //
    // Complexity: time is the sum of the times of the repairs
    //             memory is the largest memory of the repairs
    public sealed class SequentialRepair<TGene> : IRepair<TGene>
    {
        public bool PreservesIndividualLength => Repairs.All(repair => repair.PreservesIndividualLength);

        // Repairs in the order they are applied; at least one, none of them null.
        public IReadOnlyList<IRepair<TGene>> Repairs { get; }


        public SequentialRepair(IReadOnlyList<IRepair<TGene>> repairs)
        {
            RepairUtilities.ValidateRepairs(repairs, nameof(repairs));
            Repairs = repairs.ToArray();
        }

        public Individual<TGene> Repair(Individual<TGene> individual, IRandom random = null)
        {
            RepairUtilities.ValidateIndividualNotNull(individual);

            var repairedIndividual = individual;
            foreach (var repair in Repairs)
                repairedIndividual = repair.Repair(repairedIndividual, random);
            return repairedIndividual;
        }
    }
}
