using System.Collections.Generic;


namespace GeneticAlgorithms.Individuals
{
    public interface IIndividualComparer<TGene> : IComparer<Individual<TGene>>
    {
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
