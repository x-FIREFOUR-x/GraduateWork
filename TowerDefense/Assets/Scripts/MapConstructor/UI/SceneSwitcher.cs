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
            MapValidationError error = ConstructedMapRules.CheckBuildingsExist(map.GetStartBuilding() != null, map.GetEndBuilding() != null);

            if (error == MapValidationError.None)
                MapSaver.instance.SetData(map.GetTileArray(), map.GetIndexesStartBuild(), map.GetIndexesEndBuild(), out error);

            if (error != MapValidationError.None)
            {
                messageManager.ShowUncorrectMapMessage(UncorrectMapDescriptions.GetMapValidationErrorDescription(error));
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
