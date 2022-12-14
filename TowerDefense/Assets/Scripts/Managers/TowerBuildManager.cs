using UnityEngine;

public class TowerBuildManager : MonoBehaviour
{
    public static TowerBuildManager instance;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject turretPrefab;

    private GameObject chosenTower;


    public GameObject TurretPrefab
    {
        get { return turretPrefab; }
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        chosenTower = null;
    }

    public GameObject GetChosenTower()
    {
        return chosenTower;
    }

    public void SetChosenTower(GameObject tower)
    {
        chosenTower = tower;
    }

    public bool isBoughtTower()
    {
        return chosenTower != null;
    }
}
