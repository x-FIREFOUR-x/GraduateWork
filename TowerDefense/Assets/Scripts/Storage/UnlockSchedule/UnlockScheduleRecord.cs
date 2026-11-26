using System;


namespace TowerDefense.Storage.UnlockSchedule
{
    [Serializable]
    public struct UnlockScheduleRecord<TType> where TType : Enum
    {
        public TType Type;
        public int WaveNumber;
    }
}
