using UnityEngine;


namespace TowerDefense.MapConstructor.UI
{
    public class ComponentSelector : MonoBehaviour
    {
        [SerializeField]
        private GameObject selectMenu;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject pathTilePrefab;
        [SerializeField]
        private GameObject blockedTilePrefab;
        [SerializeField]
        private GameObject startBuildingPrefab;
        [SerializeField]
        private GameObject endBuildingPrefab;

        public void SelectPathTile()
        {
            MapConstructor.instance.GetComponent<MapConstructor>().SetSelectedComponent(pathTilePrefab);
            selectMenu.GetComponent<SelectMenu>().ActivePathTileButton();
        }

        public void SelectBlockedTile()
        {
            MapConstructor.instance.GetComponent<MapConstructor>().SetSelectedComponent(blockedTilePrefab);
            selectMenu.GetComponent<SelectMenu>().ActiveBlockedTileButton();
        }

        public void SelectStartBuilding()
        {
            MapConstructor.instance.GetComponent<MapConstructor>().SetSelectedComponent(startBuildingPrefab);
            selectMenu.GetComponent<SelectMenu>().ActiveStartBuildingButton();
        }

        public void SelectEndBuilding()
        {
            MapConstructor.instance.GetComponent<MapConstructor>().SetSelectedComponent(endBuildingPrefab);
            selectMenu.GetComponent<SelectMenu>().ActiveEndBuildingButton();
        }
    }

}
