using UnityEngine;

public class TowerShopMenu : MonoBehaviour
{
    private TowerBuildManager towerBuildManager;

    [Header("ShopComponents")]
    [SerializeField]
    private ShopComponent turretComponent;
    [SerializeField]
    private ShopComponent panelsTurretComponent;
    [SerializeField]
    private ShopComponent rocketLauncherComponent;
    [SerializeField]
    private ShopComponent laserTurretComponent;

    [Header("SetUp")]
    [SerializeField]
    private Color colorText;

    public static string currencySymbol = "$";

    void Start()
    {
        towerBuildManager = TowerBuildManager.instance;

        turretComponent.Initialize(colorText,
            towerBuildManager.turretPrefab.GetComponent<Tower>().Price);

        panelsTurretComponent.Initialize(colorText,
            towerBuildManager.panelsTurretPrefab.GetComponent<Tower>().Price);

        rocketLauncherComponent.Initialize(colorText,
            towerBuildManager.rocketLauncherPrefab.GetComponent<Tower>().Price);

        laserTurretComponent.Initialize(colorText, 
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
