using UnityEngine;

namespace Storage
{

    [CreateAssetMenu(fileName = "MapSizeParamsStorage", menuName = "Storage/MapSizeParamsStorage")]
    public class MapSizeParamsStorage : ScriptableObject
    {

        [field: SerializeField] public Vector3 StartMap { get; private set; }
        [field: SerializeField] public int CountTile { get; private set; }

        [field: SerializeField] public Vector3 SizeTile { get; private set; }
        [field: SerializeField] public Vector3 OffsetTile { get; private set; }

        [field: SerializeField] public Vector2Int IndexesStartBuilding { get; set; }
        [field: SerializeField] public Vector2Int IndexesEndBuilding { get; set; }
        [field: SerializeField] public Vector3 OffsetBuilding { get; private set; }
    }
}

