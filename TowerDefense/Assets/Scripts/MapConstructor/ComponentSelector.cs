using UnityEngine;

public class ComponentSelector : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject pathTilePrefab;
    [SerializeField]
    private GameObject startBuildingPrefab;
    [SerializeField]
    private GameObject endBuildingPrefab;

    public void SelectPathTile()
    {
        MapConstructor.instance.GetComponent<MapConstructor>().setSelectedComponent(pathTilePrefab);
    }

    public void SelectEndBuilding()
    {
        MapConstructor.instance.GetComponent<MapConstructor>().setSelectedComponent(endBuildingPrefab);
    }

    public void SelectStartBuilding()
    {
        MapConstructor.instance.GetComponent<MapConstructor>().setSelectedComponent(startBuildingPrefab);
    }
}
