using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float startHealth = 100f;
    private float health;

    private Transform target;
    private int wavePointIndex = 0;

    public static string enemyTag = "Enemy";

    [Header("SetUp")]
    [SerializeField]
    private Image healthBar;


    void Start()
    {
        target = WayPoints.Points[0];
        health = startHealth;
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

    private void GetNextWayPoint()
    {
        wavePointIndex++;
        if (wavePointIndex > WayPoints.Points.Count - 1)
        {
            DamageEndBuilding();
        }
        else
        {
            target = WayPoints.Points[wavePointIndex];
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        healthBar.fillAmount = health / startHealth;

        if(health <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void DamageEndBuilding()
    {
        GameObject[] endBuilding = GameObject.FindGameObjectsWithTag(EndBuilding.endBuildingTag);

        if(endBuilding[0] != null)
        {
            endBuilding[0].GetComponent<EndBuilding>().TakeDamage(10);
        }

        Destroy(gameObject);
    }
}
