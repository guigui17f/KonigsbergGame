using HedgehogTeam.EasyTouch;
using System.Collections.Generic;
using UnityEngine;

namespace GuiGui.Konigsberg.MapEditor
{
    public class MapEditorManager : MonoBehaviour
    {
        public Transform NodeCanvas;
        public Transform EdgeCanvas;
        public GameObject EditorEdge;
        public GameObject EditorNode;
        public Vector2 OriginPosition;

        public static MapEditorManager Instance { get; private set; }
        public bool EdgeEditMode { get; private set; }

        private FastPool edgePool;
        private FastPool nodePool;
        private EditorEdgeView currentEdgeView;
        private RectTransform tempNodeB;
        private string savePath;

        private void Awake()
        {
            Instance = this;
            savePath = Application.dataPath + "/Resources/MapDatas/";
            edgePool = FastPoolManager.GetPool(EditorEdge);
            nodePool = FastPoolManager.GetPool(EditorNode);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                if (EdgeEditMode)
                {
                    edgePool.FastDestroy(currentEdgeView.gameObject);
                    EasyTouch.On_TouchStart -= OnTouchStart;
                    EdgeEditMode = false;
                }
                RectTransform node = nodePool.FastInstantiate<RectTransform>();
                node.SetParent(NodeCanvas, false);
                node.anchoredPosition = OriginPosition;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                RectTransform edge = edgePool.FastInstantiate<RectTransform>();
                edge.SetParent(EdgeCanvas, false);
                edge.anchoredPosition = OriginPosition;
                currentEdgeView = edge.GetComponent<EditorEdgeView>();
                EdgeEditMode = true;
                EasyTouch.On_TouchStart += OnTouchStart;
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                CreateMapData();
            }
        }

        private void OnTouchStart(Gesture gesture)
        {
            if (gesture.pickedObject != null && gesture.pickedObject.GetComponent<EditorNodeView>() != null)
            {
                EasyTouch.On_TouchStart -= OnTouchStart;
                currentEdgeView.NodeA = gesture.pickedObject.GetComponent<RectTransform>();
                if (null == tempNodeB)
                {
                    tempNodeB = new GameObject("tempNodeB").AddComponent<RectTransform>();
                    tempNodeB.SetParent(NodeCanvas, false);
                }
                tempNodeB.gameObject.SetActive(true);
                currentEdgeView.NodeB = tempNodeB;
                EasyTouch.On_TouchDown += OnTouch;
                EasyTouch.On_TouchUp += OnTouchEnd;
            }
        }

        private void OnTouch(Gesture gesture)
        {
            if (tempNodeB != null)
            {
                //UI平面与摄像机距离为100
                tempNodeB.position = gesture.GetTouchToWorldPoint(100);
            }
        }

        private void OnTouchEnd(Gesture gesture)
        {
            EasyTouch.On_TouchDown -= OnTouch;
            EasyTouch.On_TouchUp -= OnTouchEnd;
            tempNodeB.gameObject.SetActive(false);
            if (gesture.pickedObject != null && gesture.pickedObject != currentEdgeView.NodeA.gameObject && gesture.pickedObject.GetComponent<EditorNodeView>() != null)
            {
                currentEdgeView.NodeB = gesture.pickedObject.GetComponent<RectTransform>();
                EdgeEditMode = false;
            }
            else
            {
                edgePool.FastDestroy(currentEdgeView.gameObject);
                RectTransform edge = edgePool.FastInstantiate<RectTransform>();
                edge.SetParent(EdgeCanvas, false);
                edge.anchoredPosition = OriginPosition;
                currentEdgeView = edge.GetComponent<EditorEdgeView>();
                EasyTouch.On_TouchStart += OnTouchStart;
            }
        }

        private void CreateMapData()
        {
            List<EditorNodeView> nodeList = new List<EditorNodeView>();
            NodeCanvas.GetComponentsInChildren(false, nodeList);
            List<EditorEdgeView> edgeList = new List<EditorEdgeView>();
            EdgeCanvas.GetComponentsInChildren(false, edgeList);
            int nodeCount = nodeList.Count;
            MapData data = new MapData();
            data.Nodes = new float[nodeCount * 2];
            for (int i = 0; i < nodeCount; i++)
            {
                nodeList[i].NodeId = i;
                Vector2 screenPosition = nodeList[i].NodeTransform.anchoredPosition;
                data.Nodes[i * 2] = screenPosition.x;
                data.Nodes[i * 2 + 1] = screenPosition.y;
            }

            int edgeCount = edgeList.Count;
            data.Edges = new int[edgeCount * 2];
            for (int i = 0; i < edgeCount; i++)
            {
                data.Edges[i * 2] = edgeList[i].NodeA.GetComponent<EditorNodeView>().NodeId;
                data.Edges[i * 2 + 1] = edgeList[i].NodeB.GetComponent<EditorNodeView>().NodeId;
            }

            ES2.Save(data, savePath + "mapdata_temp.bytes" + EasySaveUtil.OBFUSCATION_PARAMS);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            for (int i = 0; i < nodeCount; i++)
            {
                nodePool.FastDestroy(nodeList[i].gameObject);
            }
            for (int i = 0; i < edgeCount; i++)
            {
                edgePool.FastDestroy(edgeList[i].gameObject);
            }
        }
    }
}