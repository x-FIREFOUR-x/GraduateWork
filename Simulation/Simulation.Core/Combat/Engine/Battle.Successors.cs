using Simulation.Combat.Units;
using Simulation.Geometry;
using Simulation.InputData.Game.Units;


namespace Simulation.Combat.Engine
{
    internal sealed partial class Battle
    {
        partial void OnKilled(SimUnit unit)
        {
            if (!unit.Spec.HasSuccessor)
                return;

            int typeIndex = simulator.UnitIndex(unit.Spec.SuccessorType);
            UnitSpec spec = simulator.UnitSpec(typeIndex);
            var successor = new SimUnit(spec, typeIndex, spec.Health, unit.PathOffset, simulator.Map.SpawnPosition.Y + spec.TravelHeightOffset)
            {
                Previous = unit.Previous,
                Target = unit.Target,
                DistanceMoved = unit.DistanceMoved,
                RecoveryTimer = unit.Spec.SuccessorRecoveryTime
            };
            successor.Position = new Vec3(unit.Position.X, successor.TravelHeight, unit.Position.Z);

            unit.HasSuccessor = true;
            units.Add(successor);
        }
    }
}
