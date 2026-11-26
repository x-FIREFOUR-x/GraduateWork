using UnityEngine;


namespace TowerDefense.Main.Managers.TowerBuilders
{
    public abstract class TowerBuilder : MonoBehaviour
    {
        [SerializeField]
        protected UnlockScheduleController unlockScheduleController;
    }
}
