using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField]
    private float speed = 10f;

    private Transform target;
    private int wavePointIndex = 0;

    public static string enemyTag = "Enemy";


    void Start()
    {
        target = WayPoints.Points[0];
    }

    void Update()
    {
        Vector3 direction = target.position - transform.position;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        if(Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            GetNextWayPoint();
        }
    }

    void GetNextWayPoint()
    {
        wavePointIndex++;
        if (wavePointIndex > WayPoints.Points.Count - 1)
        {
            DamageTower();
        }
        else
        {
            target = WayPoints.Points[wavePointIndex];
        }
    }

    void DamageTower()
    {
        GameObject[] endBuilding = GameObject.FindGameObjectsWithTag(EndBuilding.endBuildingTag);

        if(endBuilding[0] != null)
        {
            endBuilding[0].GetComponent<EndBuilding>().TakeDamage(10);
        }

        Destroy(gameObject);
    }
}
