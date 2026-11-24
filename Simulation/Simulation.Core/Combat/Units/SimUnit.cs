using Simulation.Combat.Paths;
using Simulation.Geometry;
using Simulation.InputData.Game.Units;


namespace Simulation.Combat.Units
{
    internal sealed partial class SimUnit
    {
        private PathNode target;

        public float Health;
        public PathNode Previous;
        public Vec3 Position;
        public float DistanceMoved;
        public float SlowPercent;
        public float SlowTimer;
        public float RecoveryTimer;
        public bool IsActive = true;
        public bool HasSuccessor;

        public UnitSpec Spec { get; }
        public int TypeIndex { get; }
        public Vec3 PathOffset { get; }
        public float TravelHeight { get; }

        public PathNode Target
        {
            get => target;
            set
            {
                target = value;
                EdgeIndex = Previous != null && value != null ? Previous.EdgeTo(value) : -1;
                if (value == null)
                    return;
                TargetX = value.Position.X + PathOffset.X;
                TargetZ = value.Position.Z + PathOffset.Z;
            }
        }

        public int EdgeIndex { get; private set; } = -1;
        public float TargetX { get; private set; }
        public float TargetZ { get; private set; }

        public bool CanBeTargeted => IsActive && RecoveryTimer <= 0f;

        public SimUnit(UnitSpec spec, int typeIndex, float health, Vec3 pathOffset, float travelHeight)
        {
            Spec = spec;
            TypeIndex = typeIndex;
            Health = health;
            PathOffset = pathOffset;
            TravelHeight = travelHeight;
        }
    }
}
