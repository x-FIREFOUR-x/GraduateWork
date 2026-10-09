using UnityEngine;
using UnityEngine.EventSystems;


namespace TowerDefense.Main.UI
{
    public class CardClickArea : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private GameObject clickTarget;

        [SerializeField]
        private bool forwardLeftClick = true;
        [SerializeField]
        private bool forwardRightClick = true;


        public void OnPointerClick(PointerEventData eventData)
        {
            bool forward = eventData.button == PointerEventData.InputButton.Left
                ? forwardLeftClick
                : eventData.button == PointerEventData.InputButton.Right && forwardRightClick;

            if (forward)
                ExecuteEvents.Execute(clickTarget, eventData, ExecuteEvents.pointerClickHandler);
        }
    }

}
