using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefense.CameraControl
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Camera targetCamera;
        private Transform cam;


        [Header("Zoom")]
        [SerializeField]
        private float zoomSpeed = 15f;
        [SerializeField]
        private float zoomWheelStep = 2f;

        [SerializeField]
        private float minY = 25f;

        // Zooming in dollies forward along baseForward, which also lowers Y (the camera looks down);
        // convert the Y floor into a cap on how far that dolly may go.
        private float zoomOffsetMax;


        [Header("Movement")]
        [SerializeField]
        private float moveSpeed = 20f;

        // Move bounds at full zoom-in (minY); when unzoomed, the bounds change, linearly interpolated
        // down to a single point at the base position (zoomed all the way out, where no move room is needed/allowed).
        [SerializeField]
        private float zoomedInMinX = 19f;
        [SerializeField]
        private float zoomedInMaxX = 51f;
        [SerializeField]
        private float zoomedInMinZ = -4.5f;
        [SerializeField]
        private float zoomedInMaxZ = 30f;

        // Base position/rotation is the camera's initial placement in the scene.
        // It also doubles as the upper (fully zoomed-out) limit.
        private Vector3 basePosition;
        private Quaternion baseRotation;
        private Vector3 baseForward;

        private Vector3 moveDirectionX;
        private Vector3 moveDirectionZ;

        private Vector3 moveOffset;
        private float zoomOffset;

        private Vector2 externalMoveInput;
        private float externalZoomInput;


        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            cam = targetCamera.transform;

            basePosition = cam.position;
            baseRotation = cam.rotation;
            baseForward = cam.forward;

            moveDirectionX = Vector3.right;
            moveDirectionZ = Vector3.forward;

            zoomOffsetMax = baseForward.y < 0f ? (basePosition.y - minY) / -baseForward.y : Mathf.Infinity;
        }

        private void Update()
        {
            Vector2 move = Vector2.ClampMagnitude(ReadKeyboardMove() + externalMoveInput, 1f);
            moveOffset += (moveDirectionX * move.x + moveDirectionZ * move.y) * moveSpeed * Time.deltaTime;

            // Mouse wheel ticks are discrete events, so they are applied directly (not scaled by
            // deltaTime); held mobile zoom buttons are continuous, so they are deltaTime-scaled.
            float wheelZoom = IsPointerOverUI() ? 0f : Input.mouseScrollDelta.y * zoomWheelStep;
            float heldZoom = externalZoomInput * zoomSpeed * Time.deltaTime;
            zoomOffset = Mathf.Clamp(zoomOffset + wheelZoom + heldZoom, 0f, zoomOffsetMax);

            // Move bounds widen linearly as the camera zooms in, from a single point at the base
            // (zoomed-out) position up to the zoomed-in bounds.
            float t = zoomOffsetMax > 0f ? zoomOffset / zoomOffsetMax : 0f;
            float minX = Mathf.Lerp(basePosition.x, zoomedInMinX, t);
            float maxX = Mathf.Lerp(basePosition.x, zoomedInMaxX, t);
            float minZ = Mathf.Lerp(basePosition.z, zoomedInMinZ, t);
            float maxZ = Mathf.Lerp(basePosition.z, zoomedInMaxZ, t);

            Vector3 targetPosition = basePosition + moveOffset + baseForward * zoomOffset;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

            moveOffset = targetPosition - basePosition - baseForward * zoomOffset;

            cam.SetPositionAndRotation(targetPosition, baseRotation);
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private static Vector2 ReadKeyboardMove()
        {
            float x = 0f;
            float z = 0f;

            if (Input.GetKey(KeyCode.LeftArrow))
                x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow))
                x += 1f;
            if (Input.GetKey(KeyCode.UpArrow))
                z += 1f;
            if (Input.GetKey(KeyCode.DownArrow))
                z -= 1f;

            return new Vector2(x, z);
        }

        public void SetExternalMove(Vector2 direction)
        {
            externalMoveInput = Vector2.ClampMagnitude(direction, 1f);
        }

        public void SetExternalZoom(float direction)
        {
            externalZoomInput = Mathf.Clamp(direction, -1f, 1f);
        }

        public void ResetCamera()
        {
            moveOffset = Vector3.zero;
            zoomOffset = 0f;
            externalMoveInput = Vector2.zero;
            externalZoomInput = 0f;
        }
    }
}
