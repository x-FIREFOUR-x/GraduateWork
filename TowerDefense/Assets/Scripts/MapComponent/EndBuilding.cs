using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBuilding : MonoBehaviour
{
    [field:SerializeField]
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
