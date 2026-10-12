using UnityEngine;

using TMPro;


namespace TowerDefense.MapConstructor.UI
{
    public class MessageManager : MonoBehaviour
    {
        private const string UncorrectMapTitle = "Created map is incorrect";

        [SerializeField]
        private GameObject uncorrectMapMessage;
        [SerializeField]
        private TMP_Text uncorrectMapMessageText;

        public void CloseUncorrectMapMessage()
        {
            uncorrectMapMessage.SetActive(false);
        }
        public void OpenUncorrectMapMessage(string description)
        {
            uncorrectMapMessageText.text = $"{UncorrectMapTitle}\n<size=70%>{description}</size>";
            uncorrectMapMessage.SetActive(true);
        }
    }
}
