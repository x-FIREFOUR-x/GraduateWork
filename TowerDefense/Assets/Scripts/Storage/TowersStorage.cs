using System.Collections.Generic;

using UnityEngine;

namespace TowerDefense.Storage
{
    [CreateAssetMenu(fileName = "TowersStorage", menuName = "Storage/TowersStorage")]
    public class TowersStorage : ScriptableObject
    {
        [field: SerializeField] 
        public List<GameObject> Towers { get; private set; }
    }
}
