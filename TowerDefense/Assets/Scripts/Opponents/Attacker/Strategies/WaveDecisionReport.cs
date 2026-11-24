using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using TowerDefense.Main.Units;


namespace TowerDefense.Opponents.Attacker.Strategies
{
    public sealed class WaveDecisionReport
    {
        public string StrategyName { get; }

        public int WaveBudget { get; }

        public int WaveCost { get; }

        public IReadOnlyList<UnitType> WaveUnits { get; }

        public IReadOnlyList<double> WaveFitnessValues { get; }

        public double? DifficultyLevel { get; }

        public int SearchIterationCount { get; }

        public TimeSpan DecisionTime { get; }


        public WaveDecisionReport(string strategyName, int waveBudget, int waveCost, IReadOnlyList<UnitType> waveUnits, IReadOnlyList<double> waveFitnessValues, double? difficultyLevel, int searchIterationCount, TimeSpan decisionTime)
        {
            StrategyName = strategyName;
            WaveBudget = waveBudget;
            WaveCost = waveCost;
            WaveUnits = waveUnits;
            WaveFitnessValues = waveFitnessValues;
            DifficultyLevel = difficultyLevel;
            SearchIterationCount = searchIterationCount;
            DecisionTime = decisionTime;
        }

        public override string ToString()
        {
            string formattedFitnessValues = string.Join("; ", WaveFitnessValues.Select(value => value.ToString("0.###", CultureInfo.InvariantCulture)));
            string formattedDifficultyLevel = DifficultyLevel.HasValue ? DifficultyLevel.Value.ToString("0.###", CultureInfo.InvariantCulture) : "-";
            string decisionMilliseconds = DecisionTime.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture);
            return $"[{StrategyName}] budget={WaveBudget} cost={WaveCost} wave=[{string.Join(", ", WaveUnits)}] f=({formattedFitnessValues}) difficulty={formattedDifficultyLevel} iterations={SearchIterationCount} time={decisionMilliseconds}ms";
        }
    }
}
