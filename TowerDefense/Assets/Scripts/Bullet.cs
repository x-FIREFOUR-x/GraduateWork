using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform targetEnemy;

    [Header("Attributes")]
    [SerializeField]
    private float speed = 70f;

    [Header("Prefabs")]
    [SerializeField]
    public GameObject effectHitPrefab;


    public void Seek(Transform target)
    {
        targetEnemy = target;
    }

    void Update()
    {
        if(targetEnemy != null)
        {
            Vector3 direction = targetEnemy.position - transform.position;
            float distanceFrame = speed * Time.deltaTime;

            if(direction.magnitude <= distanceFrame)
            {
                HitTarget();
                return;
            }
            transform.Translate(direction.normalized * distanceFrame, Space.World);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void HitTarget()
    {
        GameObject effect = Instantiate(effectHitPrefab, transform.position, transform.rotation);
        Destroy(effect, 1f);

        Destroy(targetEnemy.gameObject);
        Destroy(gameObject);
    }
}
