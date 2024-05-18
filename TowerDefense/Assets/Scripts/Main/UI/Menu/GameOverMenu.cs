using UnityEngine;
using UnityEngine.SceneManagement;

using TowerDefense.Main.Map.Buildings;
using TowerDefense.Main.Managers.WaveSpawners;


namespace TowerDefense.Main.UI.Menu
{
    public class GameOverMenu : MonoBehaviour
    {
        [SerializeField]
        private string mainMenuScene = "MainMenuScene";

        [SerializeField]
        private EndBuilding endBuilding;
        [SerializeField]
        private WaveSpawner waveSpawner;

        [SerializeField]
        private int countWaveForWinDefender;

        [Header("Windows End Game:")]
        [SerializeField]
        private GameObject gameWinAttackerMenuUI;
        [SerializeField]
        private GameObject gameWinDefenderMenuUI;

        void Update()
        {
            if (endBuilding.Health <= 0)
            {
                GameWinAttacker();
            }

            if(waveSpawner.WaveNumber > countWaveForWinDefender)
            {
                GameWinDefender();
            }
        }

        private void GameWinAttacker()
        {
            Time.timeScale = 0f;

            gameWinAttackerMenuUI.SetActive(true);
        }

        private void GameWinDefender()
        {
            Time.timeScale = 0f;

            gameWinDefenderMenuUI.SetActive(true);
        }

        public void GoToMenu()
        {
            Time.timeScale = 1f;
            Destroy(MapSaver.instance.gameObject);
            SceneManager.LoadScene(mainMenuScene);
        }
    }

}
