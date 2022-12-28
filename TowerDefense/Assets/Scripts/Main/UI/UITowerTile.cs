using UnityEngine;

public class UITowerTile : MonoBehaviour
{
    [SerializeField]
    private GameObject ui;

    private TowerTile towerTile;

    public void SetTowerTile(TowerTile tile)
    {
        towerTile = tile;

        transform.position = towerTile.GetTowerBuildPosition();

        ui.SetActive(true);
    }

    public void Hide()
    {
        ui.SetActive(false);
    }
    
}
