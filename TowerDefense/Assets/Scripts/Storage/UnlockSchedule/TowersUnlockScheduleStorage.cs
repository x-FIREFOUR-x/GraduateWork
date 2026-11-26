using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Towers;


namespace TowerDefense.Storage.UnlockSchedule
{
    [CreateAssetMenu(fileName = nameof(TowersUnlockScheduleStorage), menuName = "Storage/" + nameof(TowersUnlockScheduleStorage))]
    public class TowersUnlockScheduleStorage : ScriptableObject
    {
        [field: SerializeField]
        public List<UnlockScheduleRecord<TowerType>> TowerUnlockScheduleRecords { get; private set; }
    }
}
