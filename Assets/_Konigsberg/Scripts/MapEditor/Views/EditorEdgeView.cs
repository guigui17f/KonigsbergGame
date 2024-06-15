using UnityEngine;

namespace GuiGui.Konigsberg.MapEditor
{
    public class EditorEdgeView : MonoBehaviour, IFastPoolItem
    {
        public RectTransform EdgeTransform;
        public RectTransform NodeA;
        public RectTransform NodeB;
        private float width;

        private void Awake()
        {
            width = EdgeTransform.sizeDelta.y;
            OnFastInstantiate();
        }

        public void OnFastInstantiate()
        {
        }

        private void Update()
        {
            if (NodeA != null && NodeB != null)
            {
                EdgeTransform.position = (NodeA.position + NodeB.position) * 0.5f;
                Vector2 positionSub = NodeB.anchoredPosition - NodeA.anchoredPosition;
                EdgeTransform.rotation = Quaternion.FromToRotation(Vector3.right, new Vector3(positionSub.x, positionSub.y, 0));
                EdgeTransform.sizeDelta = new Vector2(positionSub.magnitude, width);
            }
        }

        public void OnFastDestroy()
        {
            NodeA = null;
            NodeB = null;
            EdgeTransform.sizeDelta = new Vector2(width, width);
        }
    }
}