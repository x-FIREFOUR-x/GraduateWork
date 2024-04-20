using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private bool playerIsDefenderSerialize = true;
    private static bool playerIsDefender;

    [SerializeField]
    private int startMoney = 100;
    private static int MoneyByWave = 100;

    public static int MoneyDefender { get; private set; }
    private static int TotalMoneyDefender;

    public static int MoneyAttacker { get; private set; }

    public static int GetPlayerMoney()
    {
        return (playerIsDefender) ? MoneyDefender : MoneyAttacker;
    }

    void Start()
    {
        playerIsDefender = playerIsDefenderSerialize;

        MoneyDefender = startMoney;
        TotalMoneyDefender = startMoney;

        MoneyAttacker = startMoney;
    }

    public static void AddPlayerMoney(int number)
    {
        if (playerIsDefender)
        {
            MoneyDefender += number;
        }
        else
        {
            MoneyAttacker += number;
        }
    }

    public static void AddDefenderMoney(int number)
    {
        MoneyDefender += number;
    }

    public static void IncreaseMoneyAfterWave()
    {
        MoneyDefender += MoneyByWave;
        TotalMoneyDefender += MoneyByWave;

        MoneyAttacker = TotalMoneyDefender;
    }

    public static void IncreaseMoneyByWave(float multiplier)
    {
        MoneyByWave = (int)(MoneyByWave * multiplier);
    }
}
