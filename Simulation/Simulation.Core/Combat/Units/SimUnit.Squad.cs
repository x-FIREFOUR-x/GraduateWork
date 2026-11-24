using System.Collections.Generic;


namespace Simulation.Combat.Units
{
    internal sealed partial class SimUnit
    {
        public readonly List<SimUnit> SquadMembers = new List<SimUnit>();
        public SquadRoutePlan RoutePlan;
        public bool HasLostLeader;

        public bool IsLeader => Spec.MaxSquadMembers > 0;

        public bool CanRecruit => IsActive && SquadMembers.Count < Spec.MaxSquadMembers;
    }
}
