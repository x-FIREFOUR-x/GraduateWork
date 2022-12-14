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
    
}
