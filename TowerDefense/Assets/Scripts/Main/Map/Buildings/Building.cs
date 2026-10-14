using UnityEngine;


namespace TowerDefense.Main.Map.Buildings
{
    public class Building : MonoBehaviour
    {
        public void Initialize(Vector2Int cellWithBuilding, Vector3 sizeTile, Vector3 offsetTile, Vector3 offSetBuilding, Vector3 targetPoint)
        {
            Vector3 position = new Vector3(
                   (sizeTile.x + offsetTile.x) * cellWithBuilding.y,
                   offSetBuilding.y,
                   (sizeTile.z + offsetTile.z) * cellWithBuilding.x);

            Vector3 dirRotate = targetPoint - position;
            dirRotate.Normalize();
            Quaternion quaternion = Quaternion.LookRotation(dirRotate, Vector3.up);
            quaternion.x = 0;
            quaternion.z = 0;

            transform.SetPositionAndRotation(position, quaternion);
        }
    }

}
