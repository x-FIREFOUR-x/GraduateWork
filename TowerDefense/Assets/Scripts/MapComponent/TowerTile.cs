using UnityEngine;


public class TowerTile : MonoBehaviour
{
    private Renderer render;

    [Header("Attributes")]

    [SerializeField]
    private Color hoverColor;
    private Color unhoverColor;

    private GameObject tower;
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
        if (towerBuildManager.isBoughtTower() && tower == null)
        {
            GameObject chosenTower = towerBuildManager.GetChosenTower();
            tower = Instantiate(chosenTower, transform.position + towerOffset, transform.rotation);
            OnMouseExit();
        }
    }

    private void OnMouseEnter()
    {
        if (towerBuildManager.isBoughtTower() && tower == null)
        {
            render.material.color = hoverColor;
        }
    }

    private void OnMouseExit()
    {
        render.material.color = unhoverColor;
    }
}

