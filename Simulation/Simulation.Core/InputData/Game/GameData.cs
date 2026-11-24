using System;
using System.Collections.Generic;
using Simulation.InputData.Game.Towers;
using Simulation.InputData.Game.Units;


namespace Simulation.InputData.Game
{
    [Serializable]
    public class GameData
    {
        public List<UnitSpec> Units = new List<UnitSpec>();
        public List<TowerSpec> Towers = new List<TowerSpec>();
        public BattleSettings Settings = new BattleSettings();

        public UnitSpec Unit(string type)
        {
            foreach (UnitSpec unit in Units)
            {
                if (string.Equals(unit.Type, type, StringComparison.Ordinal))
                    return unit;
            }
            throw new ArgumentException($"Game data has no unit of type {type}.", nameof(type));
        }

        public TowerSpec Tower(string type)
        {
            foreach (TowerSpec tower in Towers)
            {
                if (string.Equals(tower.Type, type, StringComparison.Ordinal))
                    return tower;
            }
            throw new ArgumentException($"Game data has no tower of type {type}.", nameof(type));
        }
    }
}
