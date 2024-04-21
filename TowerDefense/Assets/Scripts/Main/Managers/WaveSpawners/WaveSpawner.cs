using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Enemies;


namespace TowerDefense.Main.Managers.WaveSpawners
{
    public abstract class WaveSpawner: MonoBehaviour
    {
        public Transform SpawnPoint { get; protected set; }

        protected EnemiesStorage enemiesStorage;

        [SerializeField]
        protected float timeBetweenWaves = 5f;
        public float timeToNextSpawn { get; protected set; } = 5f;

        public int WaveNumber { get; protected set; } = 1;


        protected List<EnemyType> availableEnemy;
        protected Dictionary<EnemyType, int> availablePrices;

        protected List<EnemyType> nextWave;
        protected bool isEnrolledhWave = true;


        public virtual void Initialize(Transform spawn)
        {
            SpawnPoint = spawn;
            enemiesStorage = Resources.Load<EnemiesStorage>($"{nameof(EnemiesStorage)}");
        }


        public bool WaveNotFinished()
        {
            return GameObject.FindGameObjectsWithTag(Enemy.enemyTag).Length > 0;
        }

        protected void SpawnEnemy(EnemyType enemyType)
        {
            Instantiate(enemiesStorage.Enemies[(int)enemyType], SpawnPoint.position, SpawnPoint.rotation);
        }

        protected void UpdateAvailableEnemy()
        {
            switch (WaveNumber)
            {
                case 1:
                    availableEnemy.Add(EnemyType.Standard);
                    availablePrices[EnemyType.Standard] = enemiesStorage.Enemies[(int)EnemyType.Standard].GetComponent<Enemy>().Price;
                    break;
                case 3:
                    availablePrices[EnemyType.Fast] = enemiesStorage.Enemies[(int)EnemyType.Fast].GetComponent<Enemy>().Price;
                    availableEnemy.Add(EnemyType.Fast);
                    break;
                case 5:
                    availablePrices[EnemyType.Tank] = enemiesStorage.Enemies[(int)EnemyType.Tank].GetComponent<Enemy>().Price;
                    availableEnemy.Add(EnemyType.Tank);
                    break;

                default:
                    break;
            }
        }

        protected void UpdateGameStats()
        {
            if (WaveNumber % 5 == 0)
            {
                foreach (var enemy in enemiesStorage.Enemies)
                {
                    enemy.GetComponent<Enemy>().UpgradeHealth((float)0.25);
                }
            }
        }
    }

}
