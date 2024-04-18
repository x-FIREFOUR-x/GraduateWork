using System.Collections.Generic;

using UnityEngine;

namespace TowerDefense.Storage
{
    [CreateAssetMenu(fileName = "EnemiesStorage", menuName = "Storage/EnemiesStorage")]
    public class EnemiesStorage : ScriptableObject
    {
        [field: SerializeField] 
        public List<GameObject> Enemies { get; private set; }
    }
}
