using UnityEngine;

public class TowerShop : MonoBehaviour
{
    TowerBuildManager towerBuildManager;

    [Header("ShopComponents")]
    [SerializeField]
    private GameObject turretComponent;
    [SerializeField]
    private GameObject panelsTurretComponent;
    [SerializeField]
    private GameObject rocketLauncherComponent;
    [SerializeField]
    private GameObject laserTurretComponent;

    [Header("SetUp")]
    [SerializeField]
    public Color colorText;

    public static string currencySymbol = "$";

    void Start()
    {
        towerBuildManager = TowerBuildManager.instance;

        turretComponent.GetComponent<ShopComponent>().Initialize(colorText,
            towerBuildManager.turretPrefab.GetComponent<Tower>().Price);

        panelsTurretComponent.GetComponent<ShopComponent>().Initialize(colorText,
            towerBuildManager.panelsTurretPrefab.GetComponent<Tower>().Price);

        rocketLauncherComponent.GetComponent<ShopComponent>().Initialize(colorText,
            towerBuildManager.rocketLauncherPrefab.GetComponent<Tower>().Price);

        laserTurretComponent.GetComponent<ShopComponent>().Initialize(colorText, 
            towerBuildManager.laserTurretPrefab.GetComponent<Tower>().Price);
    }

    public void ChooseTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.turretPrefab);
    }

    public void ChooseRocketLauncher()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.rocketLauncherPrefab);
    }

    public void ChoosePanelsTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.panelsTurretPrefab);
    }

    public void ChooseLaserTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.laserTurretPrefab);
    }

}
