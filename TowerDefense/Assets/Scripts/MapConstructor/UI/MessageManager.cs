using System.Collections;

using UnityEngine;

using TMPro;


namespace TowerDefense.MapConstructor.UI
{
    public class MessageManager : MonoBehaviour
    {
        private const string UncorrectMapTitle = "Created map is incorrect";

        [Header("DialogBoxMessage")]
        [SerializeField]
        private GameObject uncorrectMapMessage;
        [SerializeField]
        private TMP_Text uncorrectMapMessageText;

        [Header("NotificationMessage")]
        [SerializeField]
        private GameObject placementMessage;
        [SerializeField]
        private TMP_Text placementMessageText;
        [SerializeField]
        private float placementMessageDuration = 2.5f;

        private Coroutine hidePlacementMessageCoroutine;

        public void CloseUncorrectMapMessage()
        {
            uncorrectMapMessage.SetActive(false);
        }
        public void OpenUncorrectMapMessage(string description)
        {
            uncorrectMapMessageText.text = $"{UncorrectMapTitle}\n<size=70%>{description}</size>";
            uncorrectMapMessage.SetActive(true);
        }

        public void ShowPlacementMessage(string description)
        {
            placementMessageText.text = description;
            placementMessage.SetActive(true);

            if (hidePlacementMessageCoroutine != null)
                StopCoroutine(hidePlacementMessageCoroutine);

            hidePlacementMessageCoroutine = StartCoroutine(HidePlacementMessageAfterDelay());
        }

        private IEnumerator HidePlacementMessageAfterDelay()
        {
            yield return new WaitForSecondsRealtime(placementMessageDuration);

            placementMessage.SetActive(false);
            hidePlacementMessageCoroutine = null;
        }
    }
}
