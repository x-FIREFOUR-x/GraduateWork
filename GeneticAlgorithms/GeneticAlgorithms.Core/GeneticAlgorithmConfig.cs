using System;

namespace GeneticAlgorithms
{
    public sealed class GeneticAlgorithmConfig
    {
        public int PopulationSize { get; set; } = 20;

        public int CountOffspring { get; set; } = 1;


        public double InitialPopulationRandomShare { get; set; } = 0.7;

        public double InitialPopulationGreedyShare { get; set; } = 0.1;

        public double InitialPopulationWarmStartShare { get; set; } = 0.2;

        public int InitialPopulationGreedyCandidatePool { get; set; } = 3;


        public int TournamentSize { get; set; } = 3;


        public double MaxMutationShare { get; set; } = 0.5;

        public double MutationSmoothing { get; set; } = 0.2;

        public int MutationWindow { get; set; } = 50;

        public double MinMutationProbability { get; set; } = 0.02;

        public bool AcceptMutationOnlyIfNotWorse { get; set; } = true;


        public int? MaxIterations { get; set; }

        public TimeSpan? TimeLimit { get; set; }

        public int? StagnationIterations { get; set; } = 100;


        public int? Seed { get; set; }

        public void Validate()
        {
            if (PopulationSize < 2)
                throw new ArgumentOutOfRangeException(nameof(PopulationSize));
            if (CountOffspring < 1)
                throw new ArgumentOutOfRangeException(nameof(CountOffspring));
            if (InitialPopulationRandomShare < 0 || InitialPopulationGreedyShare < 0 || InitialPopulationWarmStartShare < 0 || InitialPopulationRandomShare + InitialPopulationGreedyShare + InitialPopulationWarmStartShare <= 0)
                throw new ArgumentException("Initialization shares must be non-negative with a positive sum.");
            if (InitialPopulationGreedyCandidatePool < 1)
                throw new ArgumentOutOfRangeException(nameof(InitialPopulationGreedyCandidatePool));
            if (MaxIterations == null && TimeLimit == null && StagnationIterations == null)
                throw new ArgumentException("At least one stop criterion must be configured.");
        }
    }
}
