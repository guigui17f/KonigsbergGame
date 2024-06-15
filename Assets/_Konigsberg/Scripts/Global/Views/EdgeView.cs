using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class EdgeView : MonoBehaviour, IFastPoolItem
    {
        public NodeView NodeA { get; private set; }
        public NodeView NodeB { get; private set; }

        private void Awake()
        {
            OnFastInstantiate();
        }

        public void OnFastInstantiate()
        {
        }

        public void RegisterNode(NodeView node)
        {
            if (null == NodeA)
            {
                NodeA = node;
            }
            else
            {
                NodeB = node;
            }
        }

        public NodeView GetNeighbourNode(NodeView originNode)
        {
            return NodeA == originNode ? NodeB : NodeA;
        }

        public void OnFastDestroy()
        {
            NodeA = null;
            NodeB = null;
        }

        private void OnDestroy()
        {
            OnFastDestroy();
        }
    }
}
