using System;
using System.Collections.Generic;
using System.Linq;

using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.State;
using TowerDefense.Opponents.Attacker.Evaluation;


namespace TowerDefense.Opponents.Attacker.Strategies
{
    public static class GreedyAttack
    {
        public static List<UnitType> Build(AttackerSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            var heuristic = new AttackHeuristicFitness(snapshot);
            var availableUnitsOrderedbyRatio = snapshot.AvailableUnits
                .OrderByDescending(unit => heuristic.UnitValue(unit) / snapshot.UnitPrices[unit])
                .ToList();

            var wave = new List<UnitType>();
            int remainingBudget = snapshot.WaveBudget;
            while (true)
            {
                int affordableIndex = availableUnitsOrderedbyRatio.FindIndex(unit => snapshot.UnitPrices[unit] <= remainingBudget);
                if (affordableIndex < 0)
                    return wave;

                UnitType unit = availableUnitsOrderedbyRatio[affordableIndex];
                wave.Add(unit);
                remainingBudget -= snapshot.UnitPrices[unit];
            }
        }
    }
}
