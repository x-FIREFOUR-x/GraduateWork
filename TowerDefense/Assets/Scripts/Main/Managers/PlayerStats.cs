using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    [SerializeField]
    private int startMoney = 100;

    void Start()
    {
        Money = startMoney;
    }

    public static void AddMoney()
    {
        Money += 100;
    }
}
