using System;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Mutations
{
    public class Mutation
    {
        public MutationType MutationType { get; private set; }
        public float Probability { get; private set; }

        public Mutation(MutationType mutationType, float probability)
        {
            MutationType = mutationType;

            if (probability < 0 && probability > 1)
                throw new ArgumentOutOfRangeException("Error! Input argument probability can take on values [0; 1]");

            Probability = probability;
        }
    }
}
