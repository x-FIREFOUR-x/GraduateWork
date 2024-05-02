using UnityEngine;


namespace TowerDefense.Main.Map.Buildings
{
    public class EndBuilding : Building
    {
        [field: SerializeField]
        public int health { get; private set; } = 100;

        public static string endBuildingTag = "EndBuilding";

        public void TakeDamage(int damage)
        {
            if (health > 0)
            {
                health -= damage;
                if (health < 0)
                    health = 0;
            }
        }
    }
}
