using UnityEngine;
public class Building : MonoBehaviour
{
    public void Initialize(Vector2 indexesTileWithBuilding, Vector3 sizeTile, Vector3 offsetTile, Vector3 offSetBuilding, Vector3 targetPoint)
    {
        Vector3 position = new Vector3(
               (sizeTile.x + offsetTile.x) * indexesTileWithBuilding.x,
               offSetBuilding.y,
               (sizeTile.z + offsetTile.z) * indexesTileWithBuilding.y);

        Vector3 dirRotate = targetPoint - position;
        dirRotate.Normalize();
        Quaternion quaternion = Quaternion.LookRotation(dirRotate, Vector3.up);
        quaternion.x = 0;
        quaternion.z = 0;

        transform.SetPositionAndRotation(position, quaternion); 
    }
}

