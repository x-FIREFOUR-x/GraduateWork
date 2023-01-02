using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    public static int TotalMoney { get; private set; }
    [SerializeField]
    private int startMoney = 100;

    void Start()
    {
        Money = startMoney;
        TotalMoney = startMoney;
    }

    public static void AddMoney()
    {
        TotalMoney += 100;
        Money += 100;
    }
}
