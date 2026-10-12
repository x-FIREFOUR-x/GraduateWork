using UnityEngine;
using UnityEngine.SceneManagement;

using TowerDefense.Main.Map.Path;
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
            if (TrySaveMap())
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(defenderGameScene);
            }
        }

        public void PlayAttackerGame()
        {
            if (TrySaveMap())
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(attackerGameScene);
            }
        }

        private bool TrySaveMap()
        {
            if (!map.IsAllBuilds())
            {
                messageManager.OpenUncorrectMapMessage(UncorrectMapDescriptions.GetMissingBuildings(map.GetStartBuilding() != null, map.GetEndBuilding() != null));
                return false;
            }

            if (!MapSaver.instance.SetData(map.GetTileArray(), map.GetIndexesStartBuild(), map.GetIndexesEndBuild(), out PathGraphBuildError error))
            {
                messageManager.OpenUncorrectMapMessage(UncorrectMapDescriptions.GetPathGraphBuildErrorDescription(error));
                return false;
            }

            return true;
        }

        public void Back()
        {
            Destroy(MapSaver.instance.gameObject);
            SceneManager.LoadScene(mainMenuScene);
        }
    }

}
