using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.MapConstructor.Component;


namespace TowerDefense.MapConstructor
{
    public class MapConstructor : MonoBehaviour
    {
        public static MapConstructor instance;

        private GameObject selectedComponent;

        private MapSizeParamsStorage mapSizeParams;

        [Header("Map")]
        [SerializeField]
        private GameObject map;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject towerTilePrefab;
        [SerializeField]
        private GameObject pathTilePrefab;
        [SerializeField]
        private GameObject blockedTilePrefab;
        [SerializeField]
        private GameObject startBuildingPrefab;
        [SerializeField]
        private GameObject endBuildingPrefab;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            selectedComponent = null;

            mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");

            map.GetComponent<Map>().Initialize(mapSizeParams.CountTile, mapSizeParams.StartMap, mapSizeParams.OffsetBuilding, StepTile());
        }


        public void SetSelectedComponent(GameObject component)
        {
            selectedComponent = component;
        }

        public void BuildComponent(GameObject mapTile)
        {
            if (selectedComponent == null)
                return;

            Vector2Int indexes = IndexesOf(mapTile);

            if (CellHoldsSelectedComponent(indexes))
            {
                ClearCell(indexes);
                return;
            }

            if (selectedComponent == blockedTilePrefab)
            {
                DestroyBuildingsAt(indexes);
                map.GetComponent<Map>().SetTile(indexes.x, indexes.y, blockedTilePrefab);
                return;
            }

            if (!IsInsideBorder(indexes))
                return;

            DestroyBuildingsAt(indexes);

            map.GetComponent<Map>().SetTile(indexes.x, indexes.y, pathTilePrefab);

            if (selectedComponent == startBuildingPrefab)
                map.GetComponent<Map>().SetStartBuilding(indexes.x, indexes.y, selectedComponent);

            if (selectedComponent == endBuildingPrefab)
                map.GetComponent<Map>().SetEndBuilding(indexes.x, indexes.y, selectedComponent);
        }


        private bool CellHoldsSelectedComponent(Vector2Int indexes)
        {
            GameObject startBuilding = map.GetComponent<Map>().GetStartBuilding();
            GameObject endBuilding = map.GetComponent<Map>().GetEndBuilding();

            if (selectedComponent == startBuildingPrefab)
                return IsBuildingAt(startBuilding, indexes);

            if (selectedComponent == endBuildingPrefab)
                return IsBuildingAt(endBuilding, indexes);

            if (IsBuildingAt(startBuilding, indexes) || IsBuildingAt(endBuilding, indexes))
                return false;

            GameObject tile = map.GetComponent<Map>().GetTile(indexes.x, indexes.y);

            if (selectedComponent == blockedTilePrefab)
                return tile.GetComponent<ConstructorBlockedTile>() != null;

            return tile.GetComponent<ConstructorPathTile>() != null;
        }

        private void ClearCell(Vector2Int indexes)
        {
            map.GetComponent<Map>().SetTile(indexes.x, indexes.y, towerTilePrefab);
            DestroyBuildingsAt(indexes);
        }

        private void DestroyBuildingsAt(Vector2Int indexes)
        {
            GameObject startBuilding = map.GetComponent<Map>().GetStartBuilding();
            if (IsBuildingAt(startBuilding, indexes))
                Destroy(startBuilding);

            GameObject endBuilding = map.GetComponent<Map>().GetEndBuilding();
            if (IsBuildingAt(endBuilding, indexes))
                Destroy(endBuilding);
        }

        private bool IsBuildingAt(GameObject building, Vector2Int indexes)
        {
            return building != null && IndexesOf(building) == indexes;
        }

        private bool IsInsideBorder(Vector2Int indexes)
        {
            return indexes.x > 0 && indexes.x < mapSizeParams.CountTile - 1
                && indexes.y > 0 && indexes.y < mapSizeParams.CountTile - 1;
        }

        private Vector2Int IndexesOf(GameObject mapObject)
        {
            Vector2Int indexes = new(Mathf.RoundToInt((mapObject.transform.position.x - mapSizeParams.StartMap.x) / StepTile().x),
                                     Mathf.RoundToInt((mapObject.transform.position.z - mapSizeParams.StartMap.z) / StepTile().z));

            return indexes;
        }

        private Vector3 StepTile()
        {
            return mapSizeParams.SizeTile + mapSizeParams.OffsetTile;
        }
    }

}
