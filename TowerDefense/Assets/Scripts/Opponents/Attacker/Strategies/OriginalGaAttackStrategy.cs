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
    public sealed class OriginalGaAttackStrategy : GeneticAttackStrategy
    {
        public override string StrategyName => "GeneticAlgorithm-original";


        public OriginalGaAttackStrategy(TimeSpan timeLimit, int? seed) : base(timeLimit, seed)
        {
        }

        protected override IFitnessFunction<UnitType> CreateFitness(AttackerSnapshot snapshot) => new AttackSimulationFitness(snapshot);

        protected override GeneticAlgorithmConfig CreateConfig() => AttackOriginalGaConfiguration.CreateConfig();

        protected override GeneticAlgorithm<UnitType> CreateAlgorithm(IFitnessFunction<UnitType> fitness, CostLimitConstraint<UnitType> constraint, IReadOnlyList<UnitType> availableUnits, GeneticAlgorithmConfig config) =>
            AttackOriginalGaConfiguration.Create(fitness, constraint, availableUnits, config);

        protected override IReadOnlyList<UnitType> SelectWave(GeneticAlgorithmResult<UnitType> result) => result.BestIndividual.Genes;
    }
}
