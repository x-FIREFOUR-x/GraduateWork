using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TowerDefense.CameraControl.UI
{
    // Builds the on-screen camera controls (touch d-pad + zoom + home, grouped together) at
    // runtime, so no per-scene UI authoring/prefab wiring is required.
    public class CameraControlsUI : MonoBehaviour
    {
        [SerializeField]
        private CameraController cameraController;

        [SerializeField]
        private Sprite zoomInIcon;
        [SerializeField]
        private Sprite zoomOutIcon;
        [SerializeField]
        private Sprite homeIcon;
        [SerializeField]
        private Sprite cameraIcon;

        private const float TopOffsetPercent = 20f;

        private const float BackgroundWidthPercent = 16f;
        private const float BackgroundHeightPercent = 28f;

        private const int Columns = 4;
        private const float MarginPercent = 1.25f;
        private const float GapPercent = 0.42f;
        private const float BackgroundPaddingPercent = 0.73f;

        private struct ButtonSpec
        {
            public string Name;
            public string Label;
            public Sprite Icon;
            public Vector2? MoveDirection;
            public float ZoomDirection;
            public bool IsHome;
        }

        private void Start()
        {
            if (cameraController == null)
                cameraController = GetComponent<CameraController>();
            if (cameraController == null)
                cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
                return;

            EnsureEventSystem();

            Transform canvas = BuildCanvas().transform;

            Canvas.ForceUpdateCanvases();
            Rect canvasRect = canvas.GetComponent<RectTransform>().rect;

            float topOffset = TopOffsetPercent / 100f * canvasRect.height;
            float margin = MarginPercent / 100f * canvasRect.width;

            RectTransform background = CreateBackground(canvas, topOffset, canvasRect.width, canvasRect.height, margin, out float buttonSize, out float gap, out float padding);

            BuildOpenCloseButton(canvas, background, buttonSize, cameraIcon);

            RectTransform controlsRoot = BuildControlsRoot(background, padding);
            BuildControlsCluster(controlsRoot, buttonSize, gap);
        }

        private Canvas BuildCanvas()
        {
            GameObject canvasObject = new GameObject("CameraControlsCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        private static RectTransform CreateBackground(Transform canvas, float topOffset, float canvasWidth, float canvasHeight, float margin, out float buttonSize, out float gap, out float padding)
        {
            float backgroundWidth = BackgroundWidthPercent / 100f * canvasWidth;
            float backgroundHeight = BackgroundHeightPercent / 100f * canvasHeight;

            gap = GapPercent / 100f * canvasWidth;
            padding = BackgroundPaddingPercent / 100f * canvasWidth;

            buttonSize = (backgroundWidth - padding * 2f - (Columns - 1) * gap) / Columns;

            float topEdge = -topOffset + padding;
            float rightEdge = -margin + padding;

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(canvas, false);
            backgroundObject.transform.SetAsFirstSibling();

            RectTransform rect = backgroundObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(backgroundWidth, backgroundHeight);
            rect.anchoredPosition = new Vector2(rightEdge, topEdge);

            Image image = backgroundObject.AddComponent<Image>();
            image.color = new Color(0.05f, 0.07f, 0.12f, 0.25f);
            image.raycastTarget = false;

            return rect;
        }

        private static RectTransform BuildControlsRoot(RectTransform background, float padding)
        {
            GameObject rootObject = new GameObject("ControlsRoot");
            rootObject.transform.SetParent(background, false);

            RectTransform rect = rootObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = new Vector2(-padding, -3 * padding);

            return rect;
        }

        private static void BuildOpenCloseButton(Transform canvas, RectTransform background, float buttonSize, Sprite cameraIcon)
        {
            GameObject buttonObject = new GameObject("OpenCloseButton");
            buttonObject.transform.SetParent(canvas, false);

            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(buttonSize, buttonSize);
            rect.anchoredPosition = background.anchoredPosition;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.05f, 0.20f, 0.25f, 0.6f);

            CreateIcon(buttonObject.transform, cameraIcon);
            Text badge = CreateStateBadge(buttonObject.transform, buttonSize);

            GameObject backgroundObject = background.gameObject;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() =>
            {
                bool nowOpen = !backgroundObject.activeSelf;
                backgroundObject.SetActive(nowOpen);
                badge.text = nowOpen ? "▼" : "▶";
            });
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void BuildControlsCluster(Transform canvas, float buttonSize, float gap)
        {
            float rowTop = 0f;
            float rowMid = rowTop - (buttonSize + gap);
            float rowBottom = rowMid - (buttonSize + gap);

            float colRight = 0f;
            float colCenter = colRight - (buttonSize + gap);
            float colLeft = colCenter - (buttonSize + gap);

            CreateButton(canvas, new ButtonSpec { Name = "ArrowUp", Label = "▲", MoveDirection = Vector2.up }, new Vector2(colCenter, rowTop), buttonSize);
            CreateButton(canvas, new ButtonSpec { Name = "ArrowLeft", Label = "◀", MoveDirection = Vector2.left }, new Vector2(colLeft, rowMid), buttonSize);
            CreateButton(canvas, new ButtonSpec { Name = "ArrowRight", Label = "▶", MoveDirection = Vector2.right }, new Vector2(colRight, rowMid), buttonSize);
            CreateButton(canvas, new ButtonSpec { Name = "ArrowDown", Label = "▼", MoveDirection = Vector2.down }, new Vector2(colCenter, rowBottom), buttonSize);

            float colZoom = colLeft - (buttonSize + gap);
            float zoomInRow = rowMid + gap / 2f + buttonSize / 2f;
            float zoomOutRow = rowMid - buttonSize - gap / 2f + buttonSize / 2f;

            CreateButton(canvas, new ButtonSpec { Name = "ZoomIn", Icon = zoomInIcon, ZoomDirection = 1f }, new Vector2(colZoom, zoomInRow), buttonSize);
            CreateButton(canvas, new ButtonSpec { Name = "ZoomOut", Icon = zoomOutIcon, ZoomDirection = -1f }, new Vector2(colZoom, zoomOutRow), buttonSize);

            Vector2 homePosition = new Vector2(colCenter, rowMid);
            CreateButton(canvas, new ButtonSpec { Name = "HomeButton", Icon = homeIcon, IsHome = true }, homePosition, buttonSize);
        }

        private void CreateButton(Transform parent, ButtonSpec spec, Vector2 topRightAnchoredPosition, float buttonSize)
        {
            GameObject buttonObject = new GameObject(spec.Name);
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(buttonSize, buttonSize);
            rect.anchoredPosition = topRightAnchoredPosition;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.10f, 0.45f, 0.55f, 0.5f);

            if (spec.Icon != null)
                CreateIcon(buttonObject.transform, spec.Icon);
            else
                CreateLabel(buttonObject.transform, spec.Label, buttonSize);

            if (spec.IsHome)
            {
                Button button = buttonObject.AddComponent<Button>();
                button.targetGraphic = image;
                button.onClick.AddListener(cameraController.ResetCamera);
                return;
            }

            HoldDirectionButton holdButton = buttonObject.AddComponent<HoldDirectionButton>();

            if (spec.MoveDirection.HasValue)
            {
                Vector2 direction = spec.MoveDirection.Value;
                holdButton.Initialize(held => cameraController.SetExternalMove(held ? direction : Vector2.zero));
            }
            else
            {
                float zoomDirection = spec.ZoomDirection;
                holdButton.Initialize(held => cameraController.SetExternalZoom(held ? zoomDirection : 0f));
            }
        }

        private static Text CreateStateBadge(Transform parent, float buttonSize)
        {
            GameObject badgeObject = new GameObject("StateBadge");
            badgeObject.transform.SetParent(parent, false);

            RectTransform rect = badgeObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.55f, 0f);
            rect.anchorMax = new Vector2(1f, 0.45f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = badgeObject.AddComponent<Text>();
            text.text = "▼";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = Mathf.RoundToInt(buttonSize * 0.3f);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
        }

        private static void CreateIcon(Transform parent, Sprite sprite)
        {
            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(parent, false);

            RectTransform rect = iconObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0.15f);
            rect.anchorMax = new Vector2(0.85f, 0.85f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = iconObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private static Text CreateLabel(Transform parent, string label, float size)
        {
            GameObject textObject = new GameObject("Label");
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = textObject.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = Mathf.RoundToInt(size * 0.45f);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;

            return text;
        }
    }
}
