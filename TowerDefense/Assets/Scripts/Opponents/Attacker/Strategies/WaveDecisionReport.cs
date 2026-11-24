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

        public IReadOnlyList<string> WaveFitnessValueNames { get; }

        public double? DifficultyLevel { get; }

        public int SearchIterationCount { get; }

        public TimeSpan DecisionTime { get; }


        public WaveDecisionReport(string strategyName, int waveBudget, int waveCost, IReadOnlyList<UnitType> waveUnits, IReadOnlyList<double> waveFitnessValues, IReadOnlyList<string> waveFitnessValueNames, double? difficultyLevel, int searchIterationCount, TimeSpan decisionTime)
        {
            StrategyName = strategyName;
            WaveBudget = waveBudget;
            WaveCost = waveCost;
            WaveUnits = waveUnits;
            WaveFitnessValues = waveFitnessValues;
            WaveFitnessValueNames = waveFitnessValueNames;
            DifficultyLevel = difficultyLevel;
            SearchIterationCount = searchIterationCount;
            DecisionTime = decisionTime;
        }

        public override string ToString()
        {
            string formattedFitnessValues = string.Join("; ", WaveFitnessValues.Select((value, index) => FormatFitnessValue(value, index)));
            string formattedDifficultyLevel = DifficultyLevel.HasValue ? DifficultyLevel.Value.ToString("0.###", CultureInfo.InvariantCulture) : "-";
            string decisionMilliseconds = DecisionTime.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture);
            return $"[{StrategyName}] budget={WaveBudget} cost={WaveCost} f=({formattedFitnessValues}) difficulty={formattedDifficultyLevel} iterations={SearchIterationCount} time={decisionMilliseconds}ms wave=[{string.Join(", ", WaveUnits)}]";
        }

        private string FormatFitnessValue(double value, int index)
        {
            string formattedValue = value.ToString("0.###", CultureInfo.InvariantCulture);
            return WaveFitnessValueNames != null && index < WaveFitnessValueNames.Count ? $"{WaveFitnessValueNames[index]}={formattedValue}" : formattedValue;
        }
    }
}
