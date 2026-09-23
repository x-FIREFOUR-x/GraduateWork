using UnityEngine;
using UnityEngine.UI;


namespace TowerDefense.MapConstructor.UI
{
    public class SelectMenu : MonoBehaviour
    {
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
            pathTileButton.GetComponent<Image>().color = unactiveColor;
            blockedTileButton.GetComponent<Image>().color = unactiveColor;
            endBuildingButton.GetComponent<Image>().color = unactiveColor;
            startBuildingButton.GetComponent<Image>().color = unactiveColor;

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
            pathTileButton.GetComponent<Image>().color = activeColor;

            blockedTileButton.GetComponent<Image>().color = unactiveColor;
            endBuildingButton.GetComponent<Image>().color = unactiveColor;
            startBuildingButton.GetComponent<Image>().color = unactiveColor;
        }

        public void ActiveBlockedTileButton()
        {
            blockedTileButton.GetComponent<Image>().color = activeColor;

            pathTileButton.GetComponent<Image>().color = unactiveColor;
            endBuildingButton.GetComponent<Image>().color = unactiveColor;
            startBuildingButton.GetComponent<Image>().color = unactiveColor;
        }

        public void ActiveStartBuildingButton()
        {
            startBuildingButton.GetComponent<Image>().color = activeColor;

            endBuildingButton.GetComponent<Image>().color = unactiveColor;
            pathTileButton.GetComponent<Image>().color = unactiveColor;
            blockedTileButton.GetComponent<Image>().color = unactiveColor;
        }

        public void ActiveEndBuildingButton()
        {
            endBuildingButton.GetComponent<Image>().color = activeColor;

            pathTileButton.GetComponent<Image>().color = unactiveColor;
            startBuildingButton.GetComponent<Image>().color = unactiveColor;
            blockedTileButton.GetComponent<Image>().color = unactiveColor;
        }
    }

}
