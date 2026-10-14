using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
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

        [Header("PlayButtons")]
        [SerializeField]
        private Image defenderGameButtonImage;
        [SerializeField]
        private Image attackerGameButtonImage;
        [SerializeField]
        private Color failedColor = new(1f, 0.15f, 0.15f);
        [SerializeField]
        private float failedHighlightDuration = 0.5f;

        private readonly Dictionary<Image, Color> normalColorsByButtonImage = new();
        private readonly Dictionary<Image, Coroutine> highlightCoroutinesByButtonImage = new();

        void Awake()
        {
            normalColorsByButtonImage[defenderGameButtonImage] = defenderGameButtonImage.color;
            normalColorsByButtonImage[attackerGameButtonImage] = attackerGameButtonImage.color;
        }

        public void PlayDefenderGame()
        {
            if (TrySaveMap())
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(defenderGameScene);
            }
            else
            {
                HighlightFailedButton(defenderGameButtonImage);
            }
        }

        public void PlayAttackerGame()
        {
            if (TrySaveMap())
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(attackerGameScene);
            }
            else
            {
                HighlightFailedButton(attackerGameButtonImage);
            }
        }

        private void HighlightFailedButton(Image buttonImage)
        {
            if (highlightCoroutinesByButtonImage.TryGetValue(buttonImage, out Coroutine highlightCoroutine))
                StopCoroutine(highlightCoroutine);

            highlightCoroutinesByButtonImage[buttonImage] = StartCoroutine(HighlightFailedButtonForDuration(buttonImage));
        }

        private IEnumerator HighlightFailedButtonForDuration(Image buttonImage)
        {
            buttonImage.color = failedColor;

            yield return new WaitForSecondsRealtime(failedHighlightDuration);

            buttonImage.color = normalColorsByButtonImage[buttonImage];
            highlightCoroutinesByButtonImage.Remove(buttonImage);
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
