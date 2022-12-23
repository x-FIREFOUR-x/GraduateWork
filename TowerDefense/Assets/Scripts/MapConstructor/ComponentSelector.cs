using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentSelector : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject pathTilePrefab;
    [SerializeField]
    private GameObject startBuilding;
    [SerializeField]
    private GameObject endBuilding;

    public void SelectPathTile()
    {
        MapConstructor.instance.GetComponent<MapConstructor>().setSelectedComponent(pathTilePrefab);
    }

    public void SelectEndBuilding()
    {
        MapConstructor.instance.GetComponent<MapConstructor>().setSelectedComponent(endBuilding);
    }

    public void SelectStartBuilding()
    {
        MapConstructor.instance.GetComponent<MapConstructor>().setSelectedComponent(startBuilding);
    }
}
