using UnityEngine;

public class TowerShop : MonoBehaviour
{
    TowerBuildManager towerBuildManager;

    void Start()
    {
        towerBuildManager = TowerBuildManager.instance;
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
