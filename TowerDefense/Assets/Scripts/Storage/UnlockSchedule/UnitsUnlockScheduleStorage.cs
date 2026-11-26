using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Units;


namespace TowerDefense.Storage.UnlockSchedule
{
    [CreateAssetMenu(fileName = nameof(UnitsUnlockScheduleStorage), menuName = "Storage/" + nameof(UnitsUnlockScheduleStorage))]
    public class UnitsUnlockScheduleStorage : ScriptableObject
    {
        [field: SerializeField]
        public List<UnlockScheduleRecord<UnitType>> UnitUnlockScheduleRecords { get; private set; }
    }
}
