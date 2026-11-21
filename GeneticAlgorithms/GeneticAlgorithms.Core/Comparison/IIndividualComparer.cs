using System.Collections.Generic;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Comparison
{
    public interface IIndividualComparer<TGene> : IComparer<Individual<TGene>>
    {
        // Whether the comparison prefers feasible individuals to infeasible ones, for example by the constraints,
        // so that infeasible individuals lose to feasible ones without a repair; false for a comparison by fitness only.
        bool PrefersFeasibleIndividuals => false;


        bool IsBetter(Individual<TGene> candidate, Individual<TGene> reference)
            => Compare(candidate, reference) > 0;

        bool IsWorse(Individual<TGene> candidate, Individual<TGene> reference)
            => Compare(candidate, reference) < 0;

        bool IsNotWorse(Individual<TGene> candidate, Individual<TGene> reference)
            => Compare(candidate, reference) >= 0;

        bool IsNotBetter(Individual<TGene> candidate, Individual<TGene> reference)
            => Compare(candidate, reference) <= 0;
    }
}
