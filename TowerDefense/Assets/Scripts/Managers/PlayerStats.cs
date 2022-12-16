using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    public static int Money;

    [field: SerializeField]
    private int startMoney = 100;

    void Start()
    {
        Money = startMoney;
    }
}
