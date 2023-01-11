using UnityEngine;
using UnityEngine.EventSystems;

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


    void Awake()
    {
        render = GetComponent<Renderer>();
        unhoverColor = render.material.color;
        towerBuildManager = TowerBuildManager.instance;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Tower != null)
        {
            towerBuildManager.SetTowerTile(this);
            return;
        }

        if (towerBuildManager.CanBuild())
        {
            towerBuildManager.BuildTower(this);
        }
        else
        {
            render.material.color = notEnoughMoney;
        }
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        render.material.color = hoverColor;
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

