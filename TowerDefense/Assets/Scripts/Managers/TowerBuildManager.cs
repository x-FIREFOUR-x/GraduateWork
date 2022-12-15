using UnityEngine;

public class TowerBuildManager : MonoBehaviour
{
    public static TowerBuildManager instance;

    private GameObject chosenTower;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject turretPrefab;
    [SerializeField]
    private GameObject rocketLauncherPrefab;
    [SerializeField]
    private GameObject panelsTurretPrefab;
    [SerializeField]
    private GameObject laserTurretPrefab;

    public GameObject TurretPrefab
    {
        get { return turretPrefab; }
    }
    public GameObject RocketLauncherPrefab
    {
        get { return rocketLauncherPrefab; }
    }
    public GameObject PanelsTurretPrefab
    {
        get { return panelsTurretPrefab; }
    }
    public GameObject LaserTurretPrefab
    {
        get { return laserTurretPrefab; }
    }


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

    public bool isBoughtTower()
    {
        return chosenTower != null;
    }
}
