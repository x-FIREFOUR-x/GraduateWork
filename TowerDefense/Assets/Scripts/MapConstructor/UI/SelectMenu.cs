using UnityEngine;
using UnityEngine.UI;


namespace TowerDefense.MapConstructor.UI
{
    public class SelectMenu : MonoBehaviour
    {
        public const string iconPath = "Window/Icon";
        public const string selectedFramePath = "SelectedFrame";

        [Header("Attributes")]
        [SerializeField]
        private Color unactiveColor;
        [SerializeField]
        private Color activeColor;

        [Header("Buttons")]
        [SerializeField]
        private GameObject pathTileButton;
        [SerializeField]
        private GameObject blockedTileButton;
        [SerializeField]
        private GameObject endBuildingButton;
        [SerializeField]
        private GameObject startBuildingButton;

        [Header("Icons")]
        [SerializeField]
        private Sprite pathTileIcon;
        [SerializeField]
        private Sprite blockedTileIcon;
        [SerializeField]
        private Sprite startBuildingIcon;
        [SerializeField]
        private Sprite endBuildingIcon;

        [SerializeField]
        private Vector2 iconSize = new Vector2(88, 88);
        [SerializeField]
        private float iconRaise = 16;
        [SerializeField]
        private float captionHeight = 28;

        void Start()
        {
            SetButtonActive(pathTileButton, false);
            SetButtonActive(blockedTileButton, false);
            SetButtonActive(endBuildingButton, false);
            SetButtonActive(startBuildingButton, false);

            AddIcon(pathTileButton, pathTileIcon);
            AddIcon(blockedTileButton, blockedTileIcon);
            AddIcon(startBuildingButton, startBuildingIcon);
            AddIcon(endBuildingButton, endBuildingIcon);
        }

        private void AddIcon(GameObject button, Sprite sprite)
        {
            if (sprite == null)
            {
                Debug.LogWarning($"{button.name} has no icon assigned, bake them with Tools/Tile/Bake Constructor Icons and drop them on this menu");
                return;
            }

            Transform laidOutIcon = button.transform.Find(iconPath);
            if (laidOutIcon != null)
            {
                laidOutIcon.GetComponent<Image>().sprite = sprite;
                return;
            }

            MoveCaptionToBottom(button);

            GameObject icon = new GameObject(sprite.name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            icon.transform.SetParent(button.transform, false);
            icon.transform.SetSiblingIndex(0);

            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = iconSize;
            rect.anchoredPosition = new Vector2(0, iconRaise);

            Image image = icon.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private void MoveCaptionToBottom(GameObject button)
        {
            TMPro.TextMeshProUGUI caption = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (caption == null)
                return;

            RectTransform rect = caption.rectTransform;
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, captionHeight);
        }

        public void ActivePathTileButton()
        {
            SetButtonActive(pathTileButton, true);

            SetButtonActive(blockedTileButton, false);
            SetButtonActive(endBuildingButton, false);
            SetButtonActive(startBuildingButton, false);
        }

        public void ActiveBlockedTileButton()
        {
            SetButtonActive(blockedTileButton, true);

            SetButtonActive(pathTileButton, false);
            SetButtonActive(endBuildingButton, false);
            SetButtonActive(startBuildingButton, false);
        }

        public void ActiveStartBuildingButton()
        {
            SetButtonActive(startBuildingButton, true);

            SetButtonActive(endBuildingButton, false);
            SetButtonActive(pathTileButton, false);
            SetButtonActive(blockedTileButton, false);
        }

        public void ActiveEndBuildingButton()
        {
            SetButtonActive(endBuildingButton, true);

            SetButtonActive(pathTileButton, false);
            SetButtonActive(startBuildingButton, false);
            SetButtonActive(blockedTileButton, false);
        }

        private void SetButtonActive(GameObject button, bool isActive)
        {
            button.GetComponent<Image>().color = isActive ? activeColor : unactiveColor;

            Transform selectedFrame = button.transform.Find(selectedFramePath);
            if (selectedFrame != null)
                selectedFrame.gameObject.SetActive(isActive);
        }
    }

}
