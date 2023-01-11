using UnityEngine;

public class TowerBuildManager : MonoBehaviour
{
    public static TowerBuildManager instance;

    private Tower chosenTower;
    private TowerTile chosenTowerTile;

    [SerializeField]
    private TowerSellMenu towerSeller;
    [SerializeField]
    private TowerShopMenu towerShop;

    [field:Header("Prefabs")]
    [field: SerializeField]
    public GameObject turretPrefab { get; private set;}
    [field: SerializeField]
    public GameObject rocketLauncherPrefab { get; private set; }
    [field: SerializeField]
    public GameObject panelsTurretPrefab { get; private set; }
    [field: SerializeField]
    public GameObject laserTurretPrefab { get; private set; }


    void Awake()
    {
        chosenTower = null;
        if (instance == null)
        {
            instance = this;
        }
    }


    public Tower GetChosenTower()
    {
        return chosenTower;
    }

    public void SetChosenTower(GameObject tower)
    {
        chosenTower = tower.GetComponent<Tower>();
        chosenTowerTile = null;

        towerSeller.Hide();
    }


    public void SetTowerTile(TowerTile towerTile)
    {
        if(chosenTowerTile == towerTile)
        {
            towerSeller.Close();
        }
        else
        {
            chosenTowerTile = towerTile;
            chosenTower = null;

            towerSeller.ActiveSellMenu(chosenTowerTile.GetTowerBuildPosition(), PriceSell());
        }
    }

    public void DisetTowerTile()
    {
        chosenTowerTile = null;
        towerShop.AllComponentSetNotSelected();
    }

    public bool CanBuild()
    {
        return chosenTower != null && PlayerStats.Money >= chosenTower.Price;
    }


    public void BuildTower(TowerTile tile)
    {
        GameObject tower = Instantiate(chosenTower.gameObject, tile.GetTowerBuildPosition(), tile.transform.rotation);
        tile.Tower = tower;

        PlayerStats.Money -= chosenTower.GetComponent<Tower>().Price;
    }

    public void DestroyTower()
    {
        PlayerStats.Money += PriceSell();
        Destroy(chosenTowerTile.Tower);
        towerShop.AllComponentSetNotSelected();
    }


    private int PriceSell()
    {
        return chosenTowerTile.Tower.GetComponent<Tower>().Price / 2;
    }
}
