using UnityEngine;

public class HeuristicsCalculator : MonoBehaviour
{
    [SerializeField]
    private string towerTag = "Tower";
    [SerializeField]
    private string pathTileTag = "PathTile";

    [SerializeField]
    private Enemy standardEnemy;
    [SerializeField]
    private Enemy tankEnemy;
    [SerializeField]
    private Enemy fastEnemy;

    public float CalculateDamageAllTowers()
    {
        float DPSAllTowers = 0;

        GameObject[] towers = GameObject.FindGameObjectsWithTag(towerTag);
        foreach (var tower in towers)
        {
            DPSAllTowers += tower.GetComponent<Tower>().DamageInSecond();
        }

        GameObject[] pathTiles = GameObject.FindGameObjectsWithTag(pathTileTag);
        float distance = (pathTiles[0].transform.localScale.x + 1) * (pathTiles.Length - 2);

        float time = distance / standardEnemy.StartSpeed;

        return DPSAllTowers;
    }

    public int TotalPlayerMoney()
    {
        int totalMoney = 0;

        GameObject[] towers = GameObject.FindGameObjectsWithTag(towerTag);
        foreach (var tower in towers)
        {
            totalMoney += tower.GetComponent<Tower>().Price;
        }

        totalMoney += PlayerStats.Money;

        return totalMoney;
    }
}
