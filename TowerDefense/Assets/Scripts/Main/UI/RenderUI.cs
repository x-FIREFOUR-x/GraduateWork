using UnityEngine;

using TowerDefense.Main.Managers;


namespace TowerDefense.Main.UI
{
    public class RenderUI : MonoBehaviour
    {
        [Header("Text field")]
        [SerializeField]
        private TMPro.TextMeshProUGUI countMoney;


        void Update()
        {
            countMoney.text = PlayerStats.GetPlayerMoney().ToString() + "$";
        }

    }

}
