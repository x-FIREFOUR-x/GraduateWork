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
    }
}
