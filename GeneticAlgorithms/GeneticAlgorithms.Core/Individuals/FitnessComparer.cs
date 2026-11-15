using System.Collections.Generic;

namespace GeneticAlgorithms.Individuals
{
    public sealed class FitnessComparer<TGene> : IIndividualComparer<TGene>
    {
        public FitnessComparer(int fitnessIndex = 0)
        {
            FitnessIndex = fitnessIndex;
        }

        public int FitnessIndex { get; }

        public int Compare(Individual<TGene> x, Individual<TGene> y)
        {
            return x.FitnessValues[FitnessIndex].CompareTo(y.FitnessValues[FitnessIndex]);
        }
    }
}
