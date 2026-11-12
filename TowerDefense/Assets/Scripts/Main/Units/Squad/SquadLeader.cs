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
        [field: SerializeField]
        public int MaxSquadMembersCount { get; private set; } = 10;
        private readonly List<Unit> squadMembers = new();
        [SerializeField]
        private Sprite squadEmblem;
        [SerializeField]
        private SquadMarker squadMarkerPrefab;


        public bool CanRecruit { get { return this != null && squadMembers.Count < MaxSquadMembersCount; } }

        protected virtual void Awake()
        {
            SquadRoutePlan = new SquadRoutePlan();
            SquadMarker = SquadMarker.CreateForSquadLeader(squadMarkerPrefab, this, squadEmblem, squadRingSizeFactor);
        }

        public bool TryRecruit(Unit unit)
        {
            if (!CanRecruit || !allowedSquadMemberUnitTypes.Contains(unit.UnitType))
                return false;

            unit.JoinSquad(SquadRoutePlan, SquadMarker);
            squadMembers.Add(unit);
            return true;
        }

        protected override PathGraphNode ChooseNextBranchNodeAtForkByStrategy(PathGraphNode forkStartNode)
        {
            PathGraphNode nextBranchNode = base.ChooseNextBranchNodeAtForkByStrategy(forkStartNode);
            SquadRoutePlan.RecordNextBranchNodeAtFork(forkStartNode, nextBranchNode);
            return nextBranchNode;
        }

        void OnDestroy()
        {
            if (!gameObject.scene.isLoaded)
                return;

            foreach (Unit squadMember in squadMembers)
            {
                if (squadMember != null)
                    squadMember.LoseSquadLeader();
            }
        }
    }

}
