using UnityEngine;
using UnityEngine.EventSystems;

using TowerDefense.Main.Managers.TowerBuilders;


namespace TowerDefense.Main.Map.Tiles
{
    public class ClickableTowerTile : TowerTile
    {
        private TowerBuildManager towerBuildManager;

        [Header("Colors:")]
        [SerializeField]
        private Color hoverColor;
        private Color unhoverColor;

        [SerializeField]
        private Color failBuildColor;

        [Header("Tower Range:")]
        [SerializeField]
        private GameObject towerRangeRing;
        private Vector3 unitSize;


        private void Awake()
        {
            render = GetComponent<Renderer>();
            unhoverColor = render.material.color;
            towerBuildManager = TowerBuildManager.instance;

            unitSize = new Vector3(towerRangeRing.transform.localScale.x, towerRangeRing.transform.localScale.y, towerRangeRing.transform.localScale.z);
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Tower != null)
            {
                ActionWhenTowerBuilded();
                return;
            }

            if (towerBuildManager.CanBuild())
            {
                BuildTower();
            }
            else
            {
                FailedBuildTower();
            }
        }

        private void OnMouseEnter()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            render.material.color = hoverColor;

            ActivateTowerRangeRing();
        }

        private void OnMouseExit()
        {
            render.material.color = unhoverColor;

            DisactivateTowerRangeRing();
        }

        private void BuildTower()
        {
            DisactivateTowerRangeRing();
            towerBuildManager.BuildTower(this);
        }

        private void FailedBuildTower()
        {
            DisactivateTowerRangeRing();
            render.material.color = failBuildColor;
        }

        private void ActionWhenTowerBuilded()
        {
            DisactivateTowerRangeRing();
            towerBuildManager.CloseOrOpenTowerSellerForTowerTile(this);
        }

        private void ActivateTowerRangeRing()
        {
            float towerShootRange = towerBuildManager.GetShootRangeChosenTower();
            towerRangeRing.transform.localScale = new Vector3(unitSize.x * towerShootRange, unitSize.y * towerShootRange, unitSize.z * towerShootRange);
            towerRangeRing.SetActive(true);
        }

        private void DisactivateTowerRangeRing()
        {
            towerRangeRing.SetActive(false);
            towerRangeRing.transform.localScale = new Vector3(unitSize.x, unitSize.y, unitSize.z);
        }
    }

}
