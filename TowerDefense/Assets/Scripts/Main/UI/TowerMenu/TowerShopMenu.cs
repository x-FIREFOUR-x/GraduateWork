using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Managers.TowerBuilders;
using TowerDefense.Main.Towers;
using TowerDefense.Storage;


namespace TowerDefense.Main.UI.TowerMenu
{
    public class TowerShopMenu : MonoBehaviour
    {
        [SerializeField]
        private List<TowerShopComponent> towerShopComponents;

        private TowerBuildManager towerBuildManager;

        public static string currencySymbol = "$";


        void Start()
        {
            towerBuildManager = TowerBuildManager.instance;

            MapSizeParamsStorage mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");
            float unitSizeTile = mapSizeParams.OffsetTile.x + mapSizeParams.SizeTile.x;
            for (int i = 0; i < towerShopComponents.Count; i++)
            {
                TowerType towerType = (TowerType)i;
                towerShopComponents[i].Initialize(towerBuildManager.GetTowerPrefab(towerType).GetComponent<Tower>().Price, unitSizeTile);
                towerShopComponents[i].ButtonSelect.onClick.AddListener(() => ChooseTower(towerType));
            }
        }

        public void ChooseTower(TowerType towerType)
        {
            if (towerShopComponents[(int)towerType].IsSelected)
            {
                towerBuildManager.DisetChosenTower();
                return;
            }

            towerBuildManager.SetChosenTower(towerBuildManager.GetTowerPrefab(towerType));

            AllComponentSetNotSelected();
            towerShopComponents[(int)towerType].SetComponentSelected(true);
        }

        public void AllComponentSetNotSelected()
        {
            foreach (var shopComponent in towerShopComponents)
            {
                shopComponent.SetComponentSelected(false);
            }
        }
    }

}
