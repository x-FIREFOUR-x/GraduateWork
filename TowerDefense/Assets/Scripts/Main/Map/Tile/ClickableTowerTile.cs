using UnityEngine;
using UnityEngine.EventSystems;

using TowerDefense.Main.Managers.TowerBuilders;


namespace TowerDefense.Main.Map.Tile
{
    public class ClickableTowerTile : TowerTile
    {
        private TowerBuildManager towerBuildManager;

        [Header("Colors:")]
        [SerializeField]
        private Color hoverColor;

        [SerializeField]
        private Color failBuildColor;

        [Header("Tower Range:")]
        [SerializeField]
        private GameObject towerRangeRing;
        private Vector3 unitSize;

        private bool wasSelectedThisTile = false;


        private void Awake()
        {
            render = GetComponent<Renderer>();
            towerBuildManager = TowerBuildManager.instance;

            unitSize = new Vector3(towerRangeRing.transform.localScale.x, towerRangeRing.transform.localScale.y, towerRangeRing.transform.localScale.z);
        }

            //Not Support in Mobile
        private void OnMouseEnter()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            TileHighlight.Apply(render, hoverColor);
            ActivateTowerRangeRing();
        }

        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            wasSelectedThisTile = true;

            TileHighlight.Apply(render, hoverColor);
            ActivateTowerRangeRing();
        }

        private void OnMouseUp()
        {
            if (EventSystem.current.IsPointerOverGameObject() || !wasSelectedThisTile)
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

            //Not Support in Mobile
        private void OnMouseExit()
        {
            wasSelectedThisTile = false;

            TileHighlight.Clear(render);
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
            TileHighlight.Replace(render, failBuildColor);
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
