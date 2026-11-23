using System.Collections.Generic;

using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;


namespace TowerDefense.Opponents
{
    public static class AdvantageMatrix
    {
        public const int MaxAdvantagePoints = 50;
        private const int MinAdvantagePoints = 0;

        private static readonly Dictionary<(UnitType, TowerType), int> AdvantagePointsByUnitOverTower = new Dictionary<(UnitType, TowerType), int>
        {
            [(UnitType.Goblin, TowerType.BallistaTower)] = 5,
            [(UnitType.Goblin, TowerType.ArcherTower)] = 2,
            [(UnitType.Goblin, TowerType.CatapultTower)] = MinAdvantagePoints,
            [(UnitType.Goblin, TowerType.IceMageTower)] = 5,

            [(UnitType.Bat, TowerType.BallistaTower)] = MaxAdvantagePoints,
            [(UnitType.Bat, TowerType.ArcherTower)] = MinAdvantagePoints,
            [(UnitType.Bat, TowerType.CatapultTower)] = MaxAdvantagePoints,
            [(UnitType.Bat, TowerType.IceMageTower)] = MinAdvantagePoints,

            [(UnitType.OrcWargRider, TowerType.BallistaTower)] = 5,
            [(UnitType.OrcWargRider, TowerType.ArcherTower)] = 5,
            [(UnitType.OrcWargRider, TowerType.CatapultTower)] = 15,
            [(UnitType.OrcWargRider, TowerType.IceMageTower)] = MinAdvantagePoints,

            [(UnitType.Troll, TowerType.BallistaTower)] = 5,
            [(UnitType.Troll, TowerType.ArcherTower)] = 5,
            [(UnitType.Troll, TowerType.CatapultTower)] = 5,
            [(UnitType.Troll, TowerType.IceMageTower)] = 5,

            [(UnitType.WraithDragonRider, TowerType.BallistaTower)] = 15,
            [(UnitType.WraithDragonRider, TowerType.ArcherTower)] = 8,
            [(UnitType.WraithDragonRider, TowerType.CatapultTower)] = 15,
            [(UnitType.WraithDragonRider, TowerType.IceMageTower)] = 5,

            [(UnitType.UrukStandardBearer, TowerType.BallistaTower)] = 5,
            [(UnitType.UrukStandardBearer, TowerType.ArcherTower)] = 12,
            [(UnitType.UrukStandardBearer, TowerType.CatapultTower)] = 3,
            [(UnitType.UrukStandardBearer, TowerType.IceMageTower)] = 5,

            [(UnitType.OrcAlphaWargRider, TowerType.BallistaTower)] = 8,
            [(UnitType.OrcAlphaWargRider, TowerType.ArcherTower)] = 10,
            [(UnitType.OrcAlphaWargRider, TowerType.CatapultTower)] = 15,
            [(UnitType.OrcAlphaWargRider, TowerType.IceMageTower)] = MinAdvantagePoints,
        };

        public static int GetAdvantagePointsUnitOverTower(UnitType unit, TowerType tower) => AdvantagePointsByUnitOverTower.TryGetValue((unit, tower), out int points) ? points : 0;

        public static int GetSumAdvantagePointsUnitOverAllTowers(UnitType unit, IReadOnlyList<TowerType> towers)
        {
            int sum = 0;
            foreach (TowerType tower in towers)
                sum += GetAdvantagePointsUnitOverTower(unit, tower);
            return sum;
        }
    }
}
