using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefense.CameraControl.UI
{
    // Reports press/release state for as long as a pointer is held down on this UI element,
    // used to drive continuous camera move/zoom from touch buttons.
    public class HoldDirectionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Action<bool> onHoldChanged;

        public void Initialize(Action<bool> onHoldChanged)
        {
            this.onHoldChanged = onHoldChanged;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onHoldChanged?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onHoldChanged?.Invoke(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onHoldChanged?.Invoke(false);
        }
    }
}
