using HedgehogTeam.EasyTouch;
using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class DebugManager : MonoBehaviour
    {
#if UNITY_EDITOR
        public RectTransform TopLeft;
        public RectTransform BottomRight;

        private void Awake()
        {
            EasyTouch.On_SimpleTap += OnClick;
        }

        private void OnClick(Gesture gesture)
        {
            Debug.Log("Click");
        }

        private void OnDestroy()
        {
            EasyTouch.On_SimpleTap -= OnClick;
        }
#endif
    }
}
