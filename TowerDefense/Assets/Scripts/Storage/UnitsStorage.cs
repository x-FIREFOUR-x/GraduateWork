using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Storage
{
    [CreateAssetMenu(fileName = "UnitsStorage", menuName = "Storage/UnitsStorage")]
    public class UnitsStorage : ScriptableObject
    {
        [field: SerializeField] 
        public List<GameObject> Units { get; private set; }
    }
}
