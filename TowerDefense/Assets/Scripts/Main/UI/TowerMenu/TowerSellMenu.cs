using UnityEngine;

using TowerDefense.Main.Managers.TowerBuilders;


namespace TowerDefense.Main.UI.TowerMenu
{
    public class TowerSellMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject ui;

        [SerializeField]
        private TMPro.TextMeshProUGUI priceSellText;

        public void ActiveSellMenu(Vector3 position, float priceSell)
        {
            transform.position = position;
            priceSellText.text = priceSell.ToString() + "$";
            ui.SetActive(true);

            TowerBuildManager.instance.DisetChosenTower();
        }

        public void Hide()
        {
            ui.SetActive(false);
        }

        public void Sell()
        {
            TowerBuildManager.instance.DestroyTower();
            Hide();
        }

        public void Close()
        {
            TowerBuildManager.instance.DisetChosenTowerTile();
            Hide();
        }

    }

}
