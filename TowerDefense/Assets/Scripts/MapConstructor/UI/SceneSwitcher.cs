using UnityEngine;
using UnityEngine.SceneManagement;

using TowerDefense.MapConstructor.Component;


namespace TowerDefense.MapConstructor.UI
{
    public class SceneSwitcher : MonoBehaviour
    {
        [SerializeField]
        private string defenderGameScene = "DefenderGameScene";
        [SerializeField]
        private string attackerGameScene = "AttackerGameScene";
        [SerializeField]
        private string mainMenuScene = "MainMenuScene";

        [SerializeField]
        private Map map;
        [SerializeField]
        private MessageManager messageManager;

        public void PlayDefenderGame()
        {
            if (!map.IsAllBuilds())
            {
                messageManager.OpenUncorrectMapMessage();
                return;
            }

            if (MapSaver.instance.SetData(map.GetTileArray(), map.GetIndexesStartBuild(), map.GetIndexesEndBuild()))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(defenderGameScene);
            }
            else
            {
                messageManager.OpenUncorrectMapMessage();
            }
        }

        public void PlayAttackerGame()
        {
            if (!map.IsAllBuilds())
            {
                messageManager.OpenUncorrectMapMessage();
                return;
            }

            if (MapSaver.instance.SetData(map.GetTileArray(), map.GetIndexesStartBuild(), map.GetIndexesEndBuild()))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(attackerGameScene);
            }
            else
            {
                messageManager.OpenUncorrectMapMessage();
            }
        }

        public void Back()
        {
            Destroy(MapSaver.instance.gameObject);
            SceneManager.LoadScene(mainMenuScene);
        }
    }

}
