using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Strategies
{
    public sealed class DefenceDecisionReport
    {
        public string StrategyName { get; }

        public int Budget { get; }

        public int DefenceCost { get; }

        public IReadOnlyList<DefenderGene> Towers { get; }

        public IReadOnlyList<double> DefenceFitnessValues { get; }

        public double? DifficultyLevel { get; }

        public int SearchIterationCount { get; }

        public TimeSpan DecisionTime { get; }


        public DefenceDecisionReport(string strategyName, int budget, int defenceCost, IReadOnlyList<DefenderGene> towers, IReadOnlyList<double> defenceFitnessValues, double? difficultyLevel, int searchIterationCount, TimeSpan decisionTime)
        {
            StrategyName = strategyName;
            Budget = budget;
            DefenceCost = defenceCost;
            Towers = towers;
            DefenceFitnessValues = defenceFitnessValues;
            DifficultyLevel = difficultyLevel;
            SearchIterationCount = searchIterationCount;
            DecisionTime = decisionTime;
        }

        public override string ToString()
        {
            string formattedFitnessValues = string.Join("; ", DefenceFitnessValues.Select(value => value.ToString("0.###", CultureInfo.InvariantCulture)));
            string formattedDifficultyLevel = DifficultyLevel.HasValue ? DifficultyLevel.Value.ToString("0.###", CultureInfo.InvariantCulture) : "-";
            string decisionMilliseconds = DecisionTime.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture);
            return $"[{StrategyName}] budget={Budget} cost={DefenceCost} towers=[{string.Join(", ", Towers)}] f=({formattedFitnessValues}) difficulty={formattedDifficultyLevel} iterations={SearchIterationCount} time={decisionMilliseconds}ms";
        }
    }
}
