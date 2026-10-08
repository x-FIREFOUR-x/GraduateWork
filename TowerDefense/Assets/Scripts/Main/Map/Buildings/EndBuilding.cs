using UnityEngine;


namespace TowerDefense.Main.Map.Buildings
{
    public class EndBuilding : Building
    {
        [field: SerializeField]
        public int Health { get; private set; } = 100;
        public int MaxHealth { get; private set; }

        public static string endBuildingTag = "EndBuilding";

        private void Awake()
        {
            MaxHealth = Health;
        }

        public void TakeDamage(int damage)
        {
            if (Health > 0)
            {
                Health -= damage;
                if (Health < 0)
                    Health = 0;
            }
        }
    }
}
