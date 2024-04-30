using System;
using System.Collections.Generic;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Mutations
{
    using Random = System.Random;

    public class MutationSelector
    {
        private Random _random = new Random();

        private Dictionary<MutationType, float> _mutationsProbabilities;

        public MutationSelector()
        {
            _mutationsProbabilities = new Dictionary<MutationType, float>();

            _mutationsProbabilities[MutationType.None] = 1.0f;
            for (int i = 1; i < (int)MutationType.SizeEnum - 1; i++)
            {
                _mutationsProbabilities[(MutationType)i] = 0.0f;
            }
        }

        public MutationSelector(List<Mutation> mutations)
        {
            if (mutations.Count != (int)MutationType.SizeEnum)
                throw new ArgumentException("Error! Not all mutations are listed");

            float totalProbability = 0;
            for (int i = 0; i < mutations.Count; i++)
            {
                totalProbability += mutations[i].Probability;
            }
            if (totalProbability != 1)
                throw new ArgumentException("Error! The sum of all mutation probabilities is not equal to 1");

            _mutationsProbabilities = new Dictionary<MutationType, float>();
            foreach (var mutation in mutations)
            {
                _mutationsProbabilities[mutation.MutationType] = mutation.Probability;
            } 
        }

        public MutationType SelectMutation()
        {
            if (_mutationsProbabilities[MutationType.None] == 1)
                return MutationType.None;

            int rand = _random.Next(0, 100);
            float lowLimit = 0;
            for (int i = 0; i < _mutationsProbabilities.Count; i++)
            {
                int probability = (int)(_mutationsProbabilities[(MutationType)i] * 100);
                if (rand >= lowLimit && rand <= probability)
                {
                    return MutationType.RandomRegenerateRange;
                }

                lowLimit = probability;
            }

            return MutationType.None;
        }

    }
}
