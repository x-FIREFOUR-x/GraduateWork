using UnityEngine;


namespace TowerDefense.Storage
{

    [CreateAssetMenu(fileName = "MapSizeParamsStorage", menuName = "Storage/MapSizeParamsStorage")]
    public class MapSizeParamsStorage : ScriptableObject
    {

        [field: SerializeField] public Vector3 StartMap { get; private set; }
        [field: SerializeField] public int CountTile { get; private set; }

        [field: SerializeField] public Vector3 SizeTile { get; private set; }
        [field: SerializeField] public Vector3 OffsetTile { get; private set; }

        [field: SerializeField, Range(0, 1)] public float BlockedTilesShare { get; private set; }

        [field: SerializeField, Min(1)] public int MinPathBranchesCount { get; private set; } = 1;
        [field: SerializeField, Min(1)] public int MaxPathBranchesCount { get; private set; } = 5;
        [field: SerializeField, Min(2)] public int MinPathBranchStartEndDistance { get; private set; } = 4;
        [field: SerializeField, Min(3)] public int MaxPathBranchLength { get; private set; } = 20;

        [field: SerializeField] public Vector2Int IndexesStartBuilding { get; private set; }
        [field: SerializeField] public Vector2Int IndexesEndBuilding { get; private set; }
        [field: SerializeField] public Vector3 OffsetBuilding { get; private set; }

        [field: SerializeField] public float WayPointHeight { get; private set; } = 2.5f;


        public Vector3 CellToPosition(Vector2Int cell)
        {
            return new Vector3(cell.y * (SizeTile.x + OffsetTile.x), WayPointHeight, cell.x * (SizeTile.z + OffsetTile.z));
        }
    }
}

