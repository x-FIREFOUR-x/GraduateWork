using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using TowerDefense.Main.Managers.WaveSpawners;
using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Storage;
using TowerDefense.Storage.UnlockSchedule;


namespace TowerDefense.Main.Managers
{
    public class UnlockScheduleController : MonoBehaviour
    {
        private const string UnlockScheduleResourcesFolder = "UnlockSchedule";

        [SerializeField]
        private WaveSpawner waveSpawner;
        [SerializeField]
        private UnitsUnlockScheduleStorage unitsUnlockScheduleStorage;
        [SerializeField]
        private TowersUnlockScheduleStorage towersUnlockScheduleStorage;

        private List<UnlockScheduleRecord<UnitType>> UnitUnlockScheduleRecords = new List<UnlockScheduleRecord<UnitType>>();
        private List<UnlockScheduleRecord<TowerType>> TowerUnlockScheduleRecords = new List<UnlockScheduleRecord<TowerType>>();

        public int CurrentWaveNumber => waveSpawner.WaveNumber;

        public IReadOnlyList<UnitType> UnlockedUnits => UnlockedTypesAt(UnitUnlockScheduleRecords, CurrentWaveNumber);

        public IReadOnlyDictionary<UnitType, int> UnitPrices { get; private set; } = new Dictionary<UnitType, int>();

        public IReadOnlyList<TowerType> UnlockedTowers => UnlockedTypesAt(TowerUnlockScheduleRecords, CurrentWaveNumber);

        public IReadOnlyDictionary<TowerType, int> TowerPrices { get; private set; } = new Dictionary<TowerType, int>();


        private void Awake()
        {
            if (waveSpawner == null)
                waveSpawner = FindAnyObjectByType<WaveSpawner>();

            unitsUnlockScheduleStorage = LoadIfMissing(unitsUnlockScheduleStorage);
            if (unitsUnlockScheduleStorage != null)
                (UnitUnlockScheduleRecords, UnitPrices) = RegisterUnlockRecords(unitsUnlockScheduleStorage.UnitUnlockScheduleRecords, Resources.Load<UnitsStorage>(nameof(UnitsStorage)).Units, (Unit unit) => unit.Price);

            towersUnlockScheduleStorage = LoadIfMissing(towersUnlockScheduleStorage);
            if (towersUnlockScheduleStorage != null)
                (TowerUnlockScheduleRecords, TowerPrices) = RegisterUnlockRecords(towersUnlockScheduleStorage.TowerUnlockScheduleRecords, Resources.Load<TowersStorage>(nameof(TowersStorage)).Towers, (Tower tower) => tower.Price);
        }

        private static TStorage LoadIfMissing<TStorage>(TStorage storage) where TStorage : ScriptableObject
        {
            if (storage != null)
                return storage;

            storage = Resources.Load<TStorage>($"{UnlockScheduleResourcesFolder}/{typeof(TStorage).Name}");
            if (storage == null)
                Debug.LogError($"{typeof(TStorage).Name} is not found.");
            return storage;
        }

        private static (List<UnlockScheduleRecord<TType>> Records, Dictionary<TType, int> Prices) RegisterUnlockRecords<TType, TComponent>(
            IEnumerable<UnlockScheduleRecord<TType>> unlocks, List<GameObject> prefabs, Func<TComponent, int> priceOf)
            where TType : Enum
            where TComponent : Component
        {
            var registeredUnlocks = new List<UnlockScheduleRecord<TType>>();
            var prices = new Dictionary<TType, int>();
            foreach (UnlockScheduleRecord<TType> unlock in unlocks.OrderBy(unlock => unlock.WaveNumber))
            {
                if (prices.ContainsKey(unlock.Type))
                    continue;

                int price = PriceOf(prefabs, Convert.ToInt32(unlock.Type), priceOf);
                if (price <= 0)
                {
                    Debug.LogWarning($"{unlock.Type} has no price and stays locked.");
                    continue;
                }

                prices[unlock.Type] = price;
                registeredUnlocks.Add(unlock);
            }
            return (registeredUnlocks, prices);
        }

        private static int PriceOf<TComponent>(List<GameObject> prefabs, int index, Func<TComponent, int> priceOf) where TComponent : Component
        {
            if (index < 0 || index >= prefabs.Count || prefabs[index] == null)
                return 0;

            TComponent component = prefabs[index].GetComponent<TComponent>();
            return component == null ? 0 : priceOf(component);
        }

        private static IReadOnlyList<TType> UnlockedTypesAt<TType>(List<UnlockScheduleRecord<TType>> unlocksScheduleRecords, int waveNumber) where TType : Enum
        {
            return unlocksScheduleRecords.Where(unlock => unlock.WaveNumber <= waveNumber).Select(unlock => unlock.Type).ToList();
        }
    }
}
