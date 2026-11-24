using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Fitness;

using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.State;


namespace TowerDefense.Opponents.Attacker.Strategies
{
    public abstract class GeneticWaveStrategy : IWaveDecisionStrategy
    {
        private readonly TimeSpan _timeLimit;
        private readonly int? _seed;
        private List<UnitType> _previousWave = new List<UnitType>();

        public WaveDecisionReport LastReport { get; private set; }

        public abstract string StrategyName { get; }

        protected virtual double? ReportedDifficultyLevel => null;


        protected GeneticWaveStrategy(TimeSpan timeLimit, int? seed)
        {
            if (timeLimit <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeLimit), timeLimit, "Time limit must be positive.");
            _timeLimit = timeLimit;
            _seed = seed;
        }

        public List<UnitType> Decide(AttackerSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            Stopwatch stopwatch = Stopwatch.StartNew();
            IFitnessFunction<UnitType> fitness = CreateFitness(snapshot);

            List<UnitType> wave = new List<UnitType>();
            int iterationCount = 0;
            if (snapshot.AvailableUnits.Any(unit => snapshot.UnitPrices[unit] <= snapshot.WaveBudget))
            {
                var constraint = new CostLimitConstraint<UnitType>(unit => snapshot.UnitPrices[unit], snapshot.WaveBudget);
                GeneticAlgorithmConfig config = CreateConfig();
                config.TimeLimit = _timeLimit;
                config.Seed = _seed;

                GeneticAlgorithm<UnitType> algorithm = CreateAlgorithm(fitness, constraint, snapshot.AvailableUnits, config);
                List<UnitType> warmStart = _previousWave.Where(unit => snapshot.UnitPrices.ContainsKey(unit)).ToList();
                GeneticAlgorithmResult<UnitType> result = algorithm.Run(warmStart.Count > 0 ? warmStart : null);

                wave = SelectWave(result).ToList();
                iterationCount = result.Progress.IterationCount;
            }

            _previousWave = wave.ToList();
            LastReport = new WaveDecisionReport(StrategyName, snapshot.WaveBudget, snapshot.Cost(wave), wave.ToList(), fitness.Evaluate(wave), (fitness as INamedFitnessValues)?.FitnessValueNames, ReportedDifficultyLevel, iterationCount, stopwatch.Elapsed);
            return wave;
        }

        public virtual void OnWaveFinished(int castleDamage)
        {
        }

        protected abstract IFitnessFunction<UnitType> CreateFitness(AttackerSnapshot snapshot);

        protected abstract GeneticAlgorithmConfig CreateConfig();

        protected abstract GeneticAlgorithm<UnitType> CreateAlgorithm(IFitnessFunction<UnitType> fitness, CostLimitConstraint<UnitType> constraint, IReadOnlyList<UnitType> availableUnits, GeneticAlgorithmConfig config);

        protected abstract IReadOnlyList<UnitType> SelectWave(GeneticAlgorithmResult<UnitType> result);
    }
}
