using Simulation.Combat.Units;
using Simulation.Geometry;
using Simulation.InputData.Game.Towers;


namespace Simulation.Combat.Towers
{
    internal sealed class SimTower
    {
        public SimUnit Target;
        public float TimeToNextFire;

        public TowerSpec Spec { get; }

        public bool[] TargetableUnits { get; }

        public Vec3 Position { get; }

        public Vec3 FirePoint => new Vec3(Position.X, Position.Y + Spec.FirePointHeight, Position.Z);

        public SimTower(TowerSpec spec, bool[] targetableUnits, Vec3 position)
        {
            Spec = spec;
            TargetableUnits = targetableUnits;
            Position = position;
        }

        public bool CanTarget(SimUnit unit) => TargetableUnits[unit.TypeIndex];
    }
}
