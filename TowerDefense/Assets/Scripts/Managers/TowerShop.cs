using UnityEngine;

public class TowerShop : MonoBehaviour
{
    TowerBuildManager towerBuildManager;

    void Start()
    {
        towerBuildManager = TowerBuildManager.instance;
    }

    public void BuyTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.TurretPrefab);
    }
    public void BuyRocketLauncher()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.RocketLauncherPrefab);
    }
    public void BuyPanelsTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.PanelsTurretPrefab);
    }

    public void BuyLaserTurret()
    {
        towerBuildManager.SetChosenTower(towerBuildManager.LaserTurretPrefab);
    }

}
