using UnityEngine;


public class TowerTile : MonoBehaviour
{
    private Renderer render;

    [Header("Attributes")]

    [SerializeField]
    private Color hoverColor;
    private Color unhoverColor;

    [SerializeField]
    private Color notEnoughMoney;

    public GameObject Tower { get; set; }
    [SerializeField]
    private Vector3 towerOffset = new Vector3(0, (float)0.5, 0);


    private TowerBuildManager towerBuildManager;

    void Start()
    {
        render = GetComponent<Renderer>();
        unhoverColor = render.material.color;
        towerBuildManager = TowerBuildManager.instance;
    }

    private void OnMouseDown()
    {
        if (towerBuildManager.IsChosenTower() && Tower == null)
        {
            if(towerBuildManager.IsMoney())
            {
                towerBuildManager.BuildTower(this);
                OnMouseExit();
            }
            else
            {
                render.material.color = notEnoughMoney;
            }
        }
    }

    private void OnMouseEnter()
    {
        if (towerBuildManager.IsChosenTower() && Tower == null)
        {
            render.material.color = hoverColor;
        }
    }

    private void OnMouseExit()
    {
        render.material.color = unhoverColor;
    }

    public Vector3 GetTowerBuildPosition()
    {
        return transform.position + towerOffset;
    }
}

