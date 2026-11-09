using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.Squad
{
    public abstract class SquadLeader : Unit
    {
        [Header("Squad")]
        [SerializeField]
        private List<UnitType> allowedSquadMemberUnitTypes = new();
        [SerializeField]
        private int maxSquadMembersCount = 10;
        private int recruitedSquadMembersCount;
        private readonly SquadRoutePlan squadRoutePlan = new();


        public bool CanRecruit { get { return this != null && recruitedSquadMembersCount < maxSquadMembersCount; } }

        public bool TryRecruit(Unit unit)
        {
            if (!CanRecruit || !allowedSquadMemberUnitTypes.Contains(unit.UnitType))
                return false;

            unit.SetSquadRoutePlan(squadRoutePlan);
            recruitedSquadMembersCount++;
            return true;
        }

        protected override PathGraphNode ChooseNextBranchNodeAtForkByStrategy(PathGraphNode forkStartNode)
        {
            PathGraphNode nextBranchNode = base.ChooseNextBranchNodeAtForkByStrategy(forkStartNode);
            squadRoutePlan.RecordNextBranchNodeAtFork(forkStartNode, nextBranchNode);
            return nextBranchNode;
        }
    }

}
