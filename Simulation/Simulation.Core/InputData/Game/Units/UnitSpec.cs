using System;
using System.Collections.Generic;


namespace Simulation.InputData.Game.Units
{
    [Serializable]
    public class UnitSpec
    {
        public string Type;
        public float Health;
        public float Speed;
        public int CastleDamage;
        public float MaxPathOffset;
        public float TravelHeightOffset;
        public float SpeedRecoveryTime;
        public ForkStrategyKind ForkStrategy;
        public List<string> SquadMemberTypes = new List<string>();
        public int MaxSquadMembers;
        public bool HasSuccessor;
        public string SuccessorType;
        public float SuccessorRecoveryTime;
    }
}
