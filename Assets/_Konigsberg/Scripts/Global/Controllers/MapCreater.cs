using System.Collections.Generic;
using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class MapCreater
    {
        private static MapCreater instance;
        public static MapCreater Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new MapCreater();
                }
                return instance;
            }
        }
        private static float edgeWidth;

        private MapCreater()
        {
            RectTransform edge = FastPoolManager.GetPool(1, null, false).FastInstantiate<RectTransform>();
            Vector2 sizeDelta = edge.sizeDelta;
            edgeWidth = sizeDelta.y;
        }

        private void CreateBase(MapData mapData, Transform nodeRoot, Transform edgeRoot, out List<NodeView> nodeList, out List<EdgeView> edgeList)
        {
            FastPool nodePool = FastPoolManager.GetPool(0, null, false);
            nodeList = new List<NodeView>();
            for (int i = 0; i < mapData.Nodes.Length; i += 2)
            {
                RectTransform node = nodePool.FastInstantiate<RectTransform>();
                node.SetParent(nodeRoot, false);
                node.anchoredPosition = new Vector2(mapData.Nodes[i], mapData.Nodes[i + 1]);
                nodeList.Add(node.GetComponent<NodeView>());
            }
            FastPool edgePool = FastPoolManager.GetPool(1, null, false);
            edgeList = new List<EdgeView>();
            for (int i = 0; i < mapData.Edges.Length; i += 2)
            {
                NodeView nodeViewA = nodeList[mapData.Edges[i]];
                NodeView nodeViewB = nodeList[mapData.Edges[i + 1]];
                RectTransform nodeA = nodeViewA.GetComponent<RectTransform>();
                RectTransform nodeB = nodeViewB.GetComponent<RectTransform>();
                RectTransform edge = edgePool.FastInstantiate<RectTransform>();
                edge.SetParent(edgeRoot, false);
                edge.position = (nodeA.position + nodeB.position) * 0.5f;
                Vector2 positionSub = nodeB.anchoredPosition - nodeA.anchoredPosition;
                edge.rotation = Quaternion.FromToRotation(Vector3.right, new Vector3(positionSub.x, positionSub.y, 0));
                edge.sizeDelta = new Vector2(positionSub.magnitude, edgeWidth);
                EdgeView edgeView = edge.GetComponent<EdgeView>();
                edgeView.RegisterNode(nodeViewA);
                edgeView.RegisterNode(nodeViewB);
                edgeList.Add(edgeView);
                nodeViewA.RegisterEdge(edgeView);
                nodeViewB.RegisterEdge(edgeView);
            }
        }

        public void CreateMap(MapData mapData, Transform nodeRoot, Transform edgeRoot, out List<NodeView> nodeList, out List<EdgeView> edgeList)
        {
            CreateBase(mapData, nodeRoot, edgeRoot, out nodeList, out edgeList);
            int nodeCount = nodeList.Count;
            for (int i = 0; i < nodeCount; i++)
            {
                nodeList[i].SetNodeData(i, false);
            }
        }

        public void LoadMap(MapData mapData, bool[] sectionState, Transform nodeRoot, Transform edgeRoot, out List<NodeView> nodeList, out List<EdgeView> edgeList)
        {
            CreateBase(mapData, nodeRoot, edgeRoot, out nodeList, out edgeList);
            int nodeCount = nodeList.Count;
            for (int i = 0; i < nodeCount; i++)
            {
                nodeList[i].SetNodeData(i, sectionState[i]);
            }
        }
    }
}
