using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class AnchorView : MonoBehaviour
    {
        public Vector2 LargeScreenAnchor;

        private void Awake()
        {
            if (Screen.height > 2400)
            {
                GetComponent<RectTransform>().anchoredPosition = LargeScreenAnchor;
            }
        }
    }
}