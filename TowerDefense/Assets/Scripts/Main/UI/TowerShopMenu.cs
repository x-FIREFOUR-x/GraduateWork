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

    

    public static string currencySymbol = "$";

    void Start()
    {
        towerBuildManager = TowerBuildManager.instance;

        turretComponent.Initialize(towerBuildManager.turretPrefab.GetComponent<Tower>().Price);

        panelsTurretComponent.Initialize(towerBuildManager.panelsTurretPrefab.GetComponent<Tower>().Price);

        rocketLauncherComponent.Initialize(towerBuildManager.rocketLauncherPrefab.GetComponent<Tower>().Price);

        laserTurretComponent.Initialize(towerBuildManager.laserTurretPrefab.GetComponent<Tower>().Price);
    }

    public void ChooseTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.turretPrefab);

        AllComponentSetNotSelected();
        turretComponent.SetComponentSelected(true);
    }

    public void ChoosePanelsTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.panelsTurretPrefab);

        AllComponentSetNotSelected();
        panelsTurretComponent.SetComponentSelected(true);
    }

    public void ChooseRocketLauncher()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.rocketLauncherPrefab);

        AllComponentSetNotSelected();
        rocketLauncherComponent.SetComponentSelected(true);
    }

    

    public void ChooseLaserTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.laserTurretPrefab);
        
        AllComponentSetNotSelected();
        laserTurretComponent.SetComponentSelected(true);
    }

    private void AllComponentSetNotSelected()
    {
        turretComponent.SetComponentSelected(false);
        panelsTurretComponent.SetComponentSelected(false);
        rocketLauncherComponent.SetComponentSelected(false);
        laserTurretComponent.SetComponentSelected(false);
    }
}
