using HedgehogTeam.EasyTouch;
using UnityEngine;

namespace GuiGui.Konigsberg.MapEditor
{
    public class EditorNodeView : MonoBehaviour, IFastPoolItem
    {
        public RectTransform NodeTransform;
        public int NodeId;

        private void Awake()
        {
            OnFastInstantiate();
        }

        public void OnFastInstantiate()
        {
            EasyTouch.On_Drag += OnDrag;
        }

        private void OnDrag(Gesture gesture)
        {
            if (gesture.pickedObject == gameObject && !MapEditorManager.Instance.EdgeEditMode)
            {
                NodeTransform.position = gesture.GetTouchToWorldPoint(100);
            }
        }

        public void OnFastDestroy()
        {
            EasyTouch.On_Drag -= OnDrag;
        }

        private void OnDestroy()
        {
            OnFastDestroy();
        }
    }
}
