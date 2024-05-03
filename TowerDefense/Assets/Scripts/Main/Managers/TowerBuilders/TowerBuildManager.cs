using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Map.Tiles;
using TowerDefense.Main.Towers;
using TowerDefense.Main.UI.TowerMenu;


namespace TowerDefense.Main.Managers.TowerBuilders
{
    public class TowerBuildManager : MonoBehaviour
    {
        public static TowerBuildManager instance;

        private Tower chosenTower;
        private TowerTile chosenTowerTile;

        private TowersStorage towersStorage;


        [SerializeField]
        private TowerSellMenu towerSeller;
        [SerializeField]
        private TowerShopMenu towerShop;


        private void Awake()
        {
            chosenTower = null;
            if (instance == null)
            {
                instance = this;
            }

            towersStorage = Resources.Load<TowersStorage>($"{nameof(TowersStorage)}");
        }

        public GameObject GetTowerPrefab(TowerType towerType)
        {
            return towersStorage.Towers[(int)towerType].gameObject;
        }

        public Tower GetChosenTower()
        {
            return chosenTower;
        }

        public void SetChosenTower(GameObject tower)
        {
            chosenTower = tower.GetComponent<Tower>();
            chosenTowerTile = null;

            towerSeller.Hide();
        }


        public void SetTowerTile(TowerTile towerTile)
        {
            if (chosenTowerTile == towerTile)
            {
                towerSeller.Close();
            }
            else
            {
                chosenTowerTile = towerTile;
                chosenTower = null;

                towerSeller.ActiveSellMenu(chosenTowerTile.GetTowerBuildPosition(), PriceSell());
            }
        }

        public void DisetTowerTile()
        {
            chosenTowerTile = null;
            towerShop.AllComponentSetNotSelected();
        }

        public bool CanBuild()
        {
            return chosenTower != null && PlayerStats.GetPlayerMoney() >= chosenTower.Price;
        }


        public void BuildTower(TowerTile tile)
        {
            GameObject tower = Instantiate(chosenTower.gameObject, tile.GetTowerBuildPosition(), tile.transform.rotation);
            tile.Tower = tower;

            PlayerStats.AddPlayerMoney(-chosenTower.GetComponent<Tower>().Price);
        }

        public void DestroyTower()
        {
            PlayerStats.AddPlayerMoney(PriceSell());
            Destroy(chosenTowerTile.Tower);
            towerShop.AllComponentSetNotSelected();
        }

        public float GetShootRangeChosenTower()
        {
            return chosenTower != null ? chosenTower.ShootRange : 0;
        }

        private int PriceSell()
        {
            return chosenTowerTile.Tower.GetComponent<Tower>().Price / 2;
        }
    }

}