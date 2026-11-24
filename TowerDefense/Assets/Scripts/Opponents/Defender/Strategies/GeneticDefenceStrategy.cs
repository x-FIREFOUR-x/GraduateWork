using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using UnityEngine;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints;
using GeneticAlgorithms.Constraints.Composite;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Fitness;

using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Strategies
{
    public abstract class GeneticDefenceStrategy : IDefenceDecisionStrategy
    {
        private const double MaxTowersPerCell = 1;

        private readonly TimeSpan _timeLimit;
        private readonly int? _seed;

        public DefenceDecisionReport LastReport { get; private set; }

        public abstract string StrategyName { get; }

        protected virtual double? ReportedDifficultyLevel => null;


        protected GeneticDefenceStrategy(TimeSpan timeLimit, int? seed)
        {
            if (timeLimit <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeLimit), timeLimit, "Time limit must be positive.");
            _timeLimit = timeLimit;
            _seed = seed;
        }

        public List<DefenderGene> Decide(DefenderSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            Stopwatch stopwatch = Stopwatch.StartNew();
            IFitnessFunction<DefenderGene> fitness = CreateFitness(snapshot);

            List<DefenderGene> towers = new List<DefenderGene>();
            int iterationCount = 0;
            List<DefenderGene> affordableGenes = snapshot.CandidateGenes
                .Where(gene => snapshot.TowerPrices[gene.Type] <= snapshot.Budget)
                .ToList();
            if (affordableGenes.Count > 0)
            {
                var costConstraint = new CostLimitConstraint<DefenderGene>(gene => snapshot.TowerPrices[gene.Type], snapshot.Budget);
                var constraint = new CompositeConstraint<DefenderGene>(
                    costConstraint,
                    new GroupWeightBoundsConstraint<DefenderGene, Vector2Int>(gene => gene.Cell, getGroupMaxWeight: cell => MaxTowersPerCell));
                GeneticAlgorithmConfig config = CreateConfig();
                config.TimeLimit = _timeLimit;
                config.Seed = _seed;

                GeneticAlgorithm<DefenderGene> algorithm = CreateAlgorithm(fitness, costConstraint, constraint, affordableGenes, config);
                GeneticAlgorithmResult<DefenderGene> result = algorithm.Run();

                towers = SelectTowers(result).ToList();
                iterationCount = result.Progress.IterationCount;
            }

            LastReport = new DefenceDecisionReport(StrategyName, snapshot.Budget, snapshot.Cost(towers), towers.ToList(), fitness.Evaluate(towers), ReportedDifficultyLevel, iterationCount, stopwatch.Elapsed);
            return towers;
        }

        public virtual void OnWaveFinished(int castleDamage)
        {
        }

        protected abstract IFitnessFunction<DefenderGene> CreateFitness(DefenderSnapshot snapshot);

        protected abstract GeneticAlgorithmConfig CreateConfig();

        protected abstract GeneticAlgorithm<DefenderGene> CreateAlgorithm(IFitnessFunction<DefenderGene> fitness, CostLimitConstraint<DefenderGene> costConstraint, IConstraint<DefenderGene> constraint, IReadOnlyList<DefenderGene> candidateGenes, GeneticAlgorithmConfig config);

        protected abstract IReadOnlyList<DefenderGene> SelectTowers(GeneticAlgorithmResult<DefenderGene> result);
    }
}
