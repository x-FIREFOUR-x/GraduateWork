using UnityEngine;

public class TowerBuildManager : MonoBehaviour
{
    public static TowerBuildManager instance;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject turretPrefab;

    private GameObject chosenTower;


    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        chosenTower = turretPrefab;
    }

    public GameObject GetChosenTower()
    {
        return chosenTower;
    }
}
