using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Managers;
using TowerDefense.Main.Units;


namespace TowerDefense.Main.UI.UnitMenu
{
    public class UnitShopMenu : MonoBehaviour
    {
        [SerializeField]
        private List<UnitShopComponent> UnitShopComponents;


        private List<UnitType> units = new List<UnitType>();

        public static string currencySymbol = "$";


        void Start()
        {
            foreach (var unitShopComponent in UnitShopComponents)
            {
                unitShopComponent.Initialize();
            }
        }

        public bool AddUnit(UnitType unit, int price, int countAddUnits = 1)
        {
            if (PlayerStats.GetPlayerMoney() < price * countAddUnits)
                return false;

            for (int i = 0; i < countAddUnits; i++)
            {
                units.Add(unit);
                PlayerStats.AddPlayerMoney(-price);
            }

            return true;
        }

        public bool SubUnit(UnitType unit, int price, int countExistUnits, int countSubUnits = 1)
        {
            if (countExistUnits < countSubUnits)
                return false;

            for (int i = 0; i < countSubUnits; i++)
            {
                units.RemoveAt(units.LastIndexOf(unit));
                PlayerStats.AddPlayerMoney(price);
            }

            return true;
        }

        public List<UnitType> TakeUnits()
        {
            List<UnitType> takedUnits = units.GetRange(0, units.Count);
            units = new List<UnitType>();

            foreach (var unitShopComponent in UnitShopComponents)
            {
                unitShopComponent.ClearCount();
            }

            return takedUnits;
        }
    }

}
