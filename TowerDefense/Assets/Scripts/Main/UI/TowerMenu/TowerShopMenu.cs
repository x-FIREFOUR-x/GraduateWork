using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Managers.TowerBuilders;
using TowerDefense.Main.Towers;


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

            for (int i = 0; i < towerShopComponents.Count; i++)
            {
                TowerType towerType = (TowerType)i;
                towerShopComponents[i].Initialize(towerBuildManager.GetTowerPrefab(towerType).GetComponent<Tower>().Price);
                towerShopComponents[i].buttonBuy.onClick.AddListener(() => ChooseTower(towerType));
            }
        }

        public void ChooseTower(TowerType towerType)
        {
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
