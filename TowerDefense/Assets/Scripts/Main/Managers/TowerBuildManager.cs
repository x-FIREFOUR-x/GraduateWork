using UnityEngine;

public class TowerBuildManager : MonoBehaviour
{
    public static TowerBuildManager instance;

    private Tower chosenTower;
    private TowerTile chosenTowerTile;

    [SerializeField]
    private GameObject uiTowerTile;

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

        uiTowerTile.GetComponent<UITowerTile>().Hide();
    }

    public void SetTowerTile(TowerTile towerTile)
    {
        if(chosenTowerTile == towerTile)
        {
            DisetTowerTile();
        }
        else
        {
            chosenTowerTile = towerTile;
            chosenTower = null;

            uiTowerTile.GetComponent<UITowerTile>().SetTowerTile(chosenTowerTile);
        }
    }

    private void DisetTowerTile()
    {
        chosenTowerTile = null;
        uiTowerTile.GetComponent<UITowerTile>().Hide();
    }

    public bool IsChosenTower()
    {
        return chosenTower != null;
    }

    public bool IsMoney()
    {
        return PlayerStats.Money >= chosenTower.Price;
    }

    public void BuildTower(TowerTile tile)
    {
        if(IsMoney())
        {
            GameObject tower = Instantiate(chosenTower.gameObject, tile.GetTowerBuildPosition(), tile.transform.rotation);
            tile.Tower = tower;

            PlayerStats.Money -= chosenTower.GetComponent<Tower>().Price;
        }
        else
        {
            Debug.Log("Enought money to build");
        }
    }
}
