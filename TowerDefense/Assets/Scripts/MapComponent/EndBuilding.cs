using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBuilding : MonoBehaviour
{
    [SerializeField]
    private int health = 100;

    public static string endBuildingTag = "EndBuilding";

    [Header("UI")]
    [SerializeField]
    private TMPro.TextMeshProUGUI healthText;

    void Update()
    {
        healthText.text = "Health: " + health.ToString();
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
