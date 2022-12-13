using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;

    private Transform target;
    private int wavePointIndex = 0;

    void Start()
    {
        target = WayPoints.Points[0];
    }

    // Update is called once per frame
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
            Destroy(gameObject);
        }
        else
        {
            target = WayPoints.Points[wavePointIndex];
        }
    }
}
