using UnityEngine;
using UnityEngine.EventSystems;

using TowerDefense.Main.Managers.TowerBuilders;


namespace TowerDefense.Main.Map.Tiles
{
    public class ClickableTowerTile : TowerTile
    {
        [Header("Colors:")]

        [SerializeField]
        private Color hoverColor;
        private Color unhoverColor;

        [SerializeField]
        private Color notEnoughMoney;


        private TowerBuildManager towerBuildManager;


        private void Awake()
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
    }

}
