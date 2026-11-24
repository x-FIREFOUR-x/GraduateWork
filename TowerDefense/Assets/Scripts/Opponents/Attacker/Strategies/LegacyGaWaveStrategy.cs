using System;
using System.Collections.Generic;

using GeneticAlgorithms;
using GeneticAlgorithms.Constraints.Implementations;
using GeneticAlgorithms.Fitness;

using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.Configurations;
using TowerDefense.Opponents.Attacker.State;
using TowerDefense.Opponents.Attacker.Evaluation;


namespace TowerDefense.Opponents.Attacker.Strategies
{
    public sealed class LegacyGaWaveStrategy : GeneticWaveStrategy
    {
        public override string StrategyName => "GeneticAlgorithm-legacy";


        public LegacyGaWaveStrategy(TimeSpan timeLimit, int? seed) : base(timeLimit, seed)
        {
        }

        protected override IFitnessFunction<UnitType> CreateFitness(AttackerSnapshot snapshot) => new WaveHeuristicFitness(snapshot);

        protected override GeneticAlgorithmConfig CreateConfig() => LegacyGaConfiguration.CreateConfig();

        protected override GeneticAlgorithm<UnitType> CreateAlgorithm(IFitnessFunction<UnitType> fitness, CostLimitConstraint<UnitType> constraint, IReadOnlyList<UnitType> availableUnits, GeneticAlgorithmConfig config) =>
            LegacyGaConfiguration.Create(fitness, constraint, availableUnits, config);

        protected override IReadOnlyList<UnitType> SelectWave(GeneticAlgorithmResult<UnitType> result) => result.BestIndividual.Genes;
    }
}
