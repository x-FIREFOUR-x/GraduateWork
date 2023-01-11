using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    public static int TotalMoney { get; private set; }
    [SerializeField]
    private int startMoney = 100;
    private static int MoneyByWave = 100;

    void Start()
    {
        Money = startMoney;
        TotalMoney = startMoney;
    }

    public static void AddMoney()
    {
        TotalMoney += MoneyByWave;
        Money += MoneyByWave;
    }

    public static void IncreaseMoneyByWave(float multiplier)
    {
        MoneyByWave += (int)(MoneyByWave * multiplier);
    }
}
