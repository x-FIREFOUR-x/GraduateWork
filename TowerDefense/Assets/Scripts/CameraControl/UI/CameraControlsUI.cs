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

        private const float TopOffsetPercent = 25f;

        private const float ButtonSize = 64f;
        private const float Gap = 8f;
        private const float Margin = 24f;

        private struct ButtonSpec
        {
            public string Name;
            public string Label;
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
            RectTransform controlsRoot = BuildControlsRoot(canvas);
            BuildControlsCluster(controlsRoot);
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

        private static RectTransform BuildControlsRoot(Transform canvas)
        {
            GameObject rootObject = new GameObject("ControlsRoot");
            rootObject.transform.SetParent(canvas, false);

            RectTransform rect = rootObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = Vector2.zero;

            Canvas.ForceUpdateCanvases();
            float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;

            float topOffset = TopOffsetPercent / 100f * canvasHeight;
            rect.anchoredPosition = new Vector2(0f, -topOffset);

            return rect;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void BuildControlsCluster(Transform canvas)
        {
            float rowTop = 0f;
            float rowMid = rowTop - (ButtonSize + Gap);
            float rowBottom = rowMid - (ButtonSize + Gap);

            float colRight = -Margin;
            float colCenter = colRight - (ButtonSize + Gap);
            float colLeft = colCenter - (ButtonSize + Gap);

            CreateButton(canvas, new ButtonSpec { Name = "ArrowUp", Label = "↑", MoveDirection = Vector2.up }, new Vector2(colCenter, rowTop));
            CreateButton(canvas, new ButtonSpec { Name = "ArrowLeft", Label = "←", MoveDirection = Vector2.left }, new Vector2(colLeft, rowMid));
            CreateButton(canvas, new ButtonSpec { Name = "ArrowRight", Label = "→", MoveDirection = Vector2.right }, new Vector2(colRight, rowMid));
            CreateButton(canvas, new ButtonSpec { Name = "ArrowDown", Label = "↓", MoveDirection = Vector2.down }, new Vector2(colCenter, rowBottom));


            float colZoom = colLeft - (ButtonSize + Gap);
            float zoomInRow = rowMid + Gap / 2f + ButtonSize / 2f;
            float zoomOutRow = rowMid - ButtonSize - Gap / 2f + ButtonSize / 2f;

            CreateButton(canvas, new ButtonSpec { Name = "ZoomIn", Label = "+", ZoomDirection = 1f }, new Vector2(colZoom, zoomInRow));
            CreateButton(canvas, new ButtonSpec { Name = "ZoomOut", Label = "-", ZoomDirection = -1f }, new Vector2(colZoom, zoomOutRow));

            Vector2 homePosition = new Vector2(colCenter, rowMid);
            CreateButton(canvas, new ButtonSpec { Name = "HomeButton", Label = "⌂", IsHome = true }, homePosition);

            float left = colZoom - ButtonSize;
            float top = rowTop;
            float bottom = rowBottom - ButtonSize;
            CreateBackground(canvas, left, colRight, top, bottom);
        }

        private static void CreateBackground(Transform parent, float left, float right, float top, float bottom)
        {
            const float padding = 14f;

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(parent, false);
            backgroundObject.transform.SetAsFirstSibling();

            RectTransform rect = backgroundObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(right - left + padding * 2f, top - bottom + padding * 2f);
            rect.anchoredPosition = new Vector2(right + padding, top + padding);

            Image image = backgroundObject.AddComponent<Image>();
            image.color = new Color(0.05f, 0.07f, 0.12f, 0.25f);
            image.raycastTarget = false;
        }

        private void CreateButton(Transform parent, ButtonSpec spec, Vector2 topRightAnchoredPosition)
        {
            GameObject buttonObject = new GameObject(spec.Name);
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(ButtonSize, ButtonSize);
            rect.anchoredPosition = topRightAnchoredPosition;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.10f, 0.45f, 0.55f, 0.5f);

            CreateLabel(buttonObject.transform, spec.Label, ButtonSize);

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

        private static void CreateLabel(Transform parent, string label, float size)
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
        }
    }
}
