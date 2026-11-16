using System;


namespace GeneticAlgorithms.Individuals
{
    public sealed class FitnessComparer<TGene> : IIndividualComparer<TGene>
    {
        public int FitnessIndex { get; }

        public FitnessComparer(int fitnessIndex = 0)
        {
            if (fitnessIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(fitnessIndex), fitnessIndex, "Fitness index must be non-negative.");
            FitnessIndex = fitnessIndex;
        }

        public int Compare(Individual<TGene> x, Individual<TGene> y)
        {
            return x.FitnessValues[FitnessIndex].CompareTo(y.FitnessValues[FitnessIndex]);
        }
    }
}
