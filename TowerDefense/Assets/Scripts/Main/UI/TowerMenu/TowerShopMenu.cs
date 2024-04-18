using UnityEngine;

public class TowerShopMenu : MonoBehaviour
{
    private TowerBuildManager towerBuildManager;

    [Header("ShopComponents")]
    [SerializeField]
    private TowerShopComponent turretComponent;
    [SerializeField]
    private TowerShopComponent panelsTurretComponent;
    [SerializeField]
    private TowerShopComponent rocketLauncherComponent;
    [SerializeField]
    private TowerShopComponent laserTurretComponent;

    
    public static string currencySymbol = "$";


    void Start()
    {
        towerBuildManager = TowerBuildManager.instance;

        turretComponent.Initialize(towerBuildManager.GetTowerPrefab(TowerType.Turret).GetComponent<Tower>().Price);
        turretComponent.buttonBuy.onClick.AddListener(() => ChooseTower(TowerType.Turret));

        panelsTurretComponent.Initialize(towerBuildManager.GetTowerPrefab(TowerType.PanelsTurret).GetComponent<Tower>().Price);
        panelsTurretComponent.buttonBuy.onClick.AddListener(() => ChooseTower(TowerType.PanelsTurret));

        rocketLauncherComponent.Initialize(towerBuildManager.GetTowerPrefab(TowerType.RocketLauncher).GetComponent<Tower>().Price);
        rocketLauncherComponent.buttonBuy.onClick.AddListener(() => ChooseTower(TowerType.RocketLauncher));

        laserTurretComponent.Initialize(towerBuildManager.GetTowerPrefab(TowerType.LaserTurret).GetComponent<Tower>().Price);
        laserTurretComponent.buttonBuy.onClick.AddListener(() => ChooseTower(TowerType.LaserTurret));
    }

    public void ChooseTower(TowerType towerType)
    {
        towerBuildManager.SetChosenTower(towerBuildManager.GetTowerPrefab(towerType));

        AllComponentSetNotSelected();
        switch (towerType)
        {
            case TowerType.Turret:
                turretComponent.SetComponentSelected(true);
                break;
            case TowerType.PanelsTurret:
                panelsTurretComponent.SetComponentSelected(true);
                break;
            case TowerType.RocketLauncher:
                rocketLauncherComponent.SetComponentSelected(true);
                break;
            case TowerType.LaserTurret:
                laserTurretComponent.SetComponentSelected(true);
                break;
            default:
                break;
        }
    }

    public void AllComponentSetNotSelected()
    {
        turretComponent.SetComponentSelected(false);
        panelsTurretComponent.SetComponentSelected(false);
        rocketLauncherComponent.SetComponentSelected(false);
        laserTurretComponent.SetComponentSelected(false);
    }
}
