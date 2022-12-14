using UnityEngine;

public class TowerBuildManager : MonoBehaviour
{
    public static TowerBuildManager instance;

    private GameObject chosenTower;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject turretPrefab;
    public GameObject TurretPrefab
    {
        get { return turretPrefab; }
    }

    [SerializeField]
    private GameObject rocketLauncherPrefab;
    public GameObject RocketLauncherPrefab
    {
        get { return rocketLauncherPrefab; }
    }

    [SerializeField]
    private GameObject panelsTurretPrefab;
    public GameObject PanelsTurretPrefab
    {
        get { return panelsTurretPrefab; }
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
