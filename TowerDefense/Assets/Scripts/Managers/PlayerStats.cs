using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    [SerializeField]
    private int startMoney = 1000;

    void Start()
    {
        Money = startMoney;
    }
}
