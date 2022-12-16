using UnityEngine;

public class TowerBuildManager : MonoBehaviour
{
    public static TowerBuildManager instance;

    private GameObject chosenTower;

    
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
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        chosenTower = null;
    }

    public GameObject GetChosenTower()
    {
        return chosenTower;
    }

    public void SetChosenTower(GameObject tower)
    {
        chosenTower = tower;
    }

    public bool IsChosenTower()
    {
        return chosenTower != null;
    }

    public bool IsMoney()
    {
        return PlayerStats.Money >= chosenTower.GetComponent<Tower>().Price;
    }

    public void BuildTower(TowerTile tile)
    {
        if(IsMoney())
        {
            GameObject tower = Instantiate(chosenTower, tile.GetTowerBuildPosition(), tile.transform.rotation);
            tile.Tower = tower;

            PlayerStats.Money -= chosenTower.GetComponent<Tower>().Price;
        }
        else
        {
            Debug.Log("Enought money to build");
        }
    }
}
