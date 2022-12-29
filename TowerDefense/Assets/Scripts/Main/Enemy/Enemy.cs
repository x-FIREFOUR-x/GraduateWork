using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField]
    private float startHealth = 100f;
    private float health;

    [field:SerializeField]
    public float StartSpeed { get; private set; } = 10f;
    private float speed;

    [SerializeField]
    private int damage = 10;

    [field: SerializeField]
    public float Price { get; private set; } = 25;


    private Transform target;
    public int WayPointIndex { get; private set; } = 0;

    public static string enemyTag = "Enemy";

    [Header("SetUp")]
    [SerializeField]
    private Image healthBar;


    void Start()
    {
        target = WayPoints.Points[0];
        health = startHealth;
        speed = StartSpeed;
    }

    void Update()
    {
        Vector3 direction = target.position - transform.position;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        if(Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            GetNextWayPoint();
        }

        speed = StartSpeed;
    }

    private void GetNextWayPoint()
    {
        WayPointIndex++;
        if (WayPointIndex > WayPoints.Points.Count - 1)
        {
            DamageEndBuilding();
        }
        else
        {
            target = WayPoints.Points[WayPointIndex];
        }
    }

    private void DamageEndBuilding()
    {
        GameObject[] endBuilding = GameObject.FindGameObjectsWithTag(EndBuilding.endBuildingTag);

        if(endBuilding[0] != null)
        {
            endBuilding[0].GetComponent<EndBuilding>().TakeDamage(damage);
        }

        Destroy(gameObject);
    }


    public void TakeDamage(float damage)
    {
        health -= damage;

        healthBar.fillAmount = health / startHealth;

        if (health <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void Slow(float percentSlowing)
    {
        speed = StartSpeed * (1 - percentSlowing);
    }
}
