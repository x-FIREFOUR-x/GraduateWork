using System.Collections;

using UnityEngine;

using TMPro;


namespace TowerDefense.MapConstructor.UI
{
    public class MessageManager : MonoBehaviour
    {
        [Header("NotificationMessage")]
        [SerializeField]
        private GameObject uncorrectMapMessage;
        [SerializeField]
        private TMP_Text uncorrectMapMessageText;
        [SerializeField]
        private float uncorrectMapMessageDuration = 2.5f;

        private Coroutine hideUncorrectMapMessageCoroutine;

        public void ShowUncorrectMapMessage(string description)
        {
            uncorrectMapMessageText.text = description;
            uncorrectMapMessage.SetActive(true);

            if (hideUncorrectMapMessageCoroutine != null)
                StopCoroutine(hideUncorrectMapMessageCoroutine);

            hideUncorrectMapMessageCoroutine = StartCoroutine(HideUncorrectMapMessageAfterDelay());
        }

        private IEnumerator HideUncorrectMapMessageAfterDelay()
        {
            yield return new WaitForSecondsRealtime(uncorrectMapMessageDuration);

            uncorrectMapMessage.SetActive(false);
            hideUncorrectMapMessageCoroutine = null;
        }
    }
}
