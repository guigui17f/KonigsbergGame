using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class MapDataGenerator
    {
        private static MapDataGenerator instance;
        public static MapDataGenerator Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new MapDataGenerator();
                }
                return instance;
            }
        }

        public MapData CreateNewData(int sectionId, Vector2 topLeft, Vector2 bottomRight)
        {
            int maxSectionId = (ProjectConst.MAP_MAX_ROW + ProjectConst.MAP_MAX_COLUMN - ProjectConst.MAP_MIN_ROW - ProjectConst.MAP_MIN_COLUMN + 1) * (ProjectConst.MAP_MAX_EDGE - ProjectConst.MAP_MIN_EDGE + 1) - 1;
            int rowCount;
            int columnCount;
            int edgeCount;
            if (sectionId >= maxSectionId)
            {
                rowCount = ProjectConst.MAP_MAX_ROW;
                columnCount = ProjectConst.MAP_MAX_COLUMN;
                edgeCount = ProjectConst.MAP_MAX_EDGE;
            }
            else
            {
                rowCount = ProjectConst.MAP_MIN_ROW;
                columnCount = ProjectConst.MAP_MIN_COLUMN;
                edgeCount = ProjectConst.MAP_MIN_EDGE;
                for (int i = 0; i < sectionId; i++)
                {
                    edgeCount++;
                    if (edgeCount > ProjectConst.MAP_MAX_EDGE)
                    {
                        if (rowCount <= columnCount)
                        {
                            rowCount++;
                        }
                        else
                        {
                            columnCount++;
                        }
                        edgeCount = ProjectConst.MAP_MIN_EDGE;
                    }
                }
            }
            NodeData[,] originMapData;
            bool success = false;
            do
            {
                originMapData = CreateMapTopology(rowCount, columnCount, edgeCount);
                success = CheckMapConnectivity(originMapData) && CheckMapAchievable(originMapData);
            } while (!success);
            return TranslateMapTopology(originMapData, topLeft, bottomRight);
        }

        public NodeData[,] CreateMapTopology(int rowCount, int columnCount, int commonMaxEdgeCount)
        {
#if UNITY_EDITOR
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
#endif
            //边界点最大边数为4
            int borderMaxEdgeCount = Mathf.Min(commonMaxEdgeCount, 4);
            NodeData[,] nodes = new NodeData[rowCount, columnCount];
            //初始化
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    NodeData newNode = new NodeData();
                    //正右方为0，顺时针排序
                    newNode.Edges = new EdgeType[8];
                    if (i == 0)
                    {
                        newNode.Edges[5] = EdgeType.OutOfRange;
                        newNode.Edges[6] = EdgeType.OutOfRange;
                        newNode.Edges[7] = EdgeType.OutOfRange;
                        newNode.BorderCount++;
                    }
                    else if (i == rowCount - 1)
                    {
                        newNode.Edges[1] = EdgeType.OutOfRange;
                        newNode.Edges[2] = EdgeType.OutOfRange;
                        newNode.Edges[3] = EdgeType.OutOfRange;
                        newNode.BorderCount++;
                    }
                    if (j == 0)
                    {
                        newNode.Edges[3] = EdgeType.OutOfRange;
                        newNode.Edges[4] = EdgeType.OutOfRange;
                        newNode.Edges[5] = EdgeType.OutOfRange;
                        newNode.BorderCount++;
                    }
                    else if (j == columnCount - 1)
                    {
                        newNode.Edges[0] = EdgeType.OutOfRange;
                        newNode.Edges[1] = EdgeType.OutOfRange;
                        newNode.Edges[7] = EdgeType.OutOfRange;
                        newNode.BorderCount++;
                    }
                    nodes[i, j] = newNode;
                }
            }
            //连接节点
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    NodeData currentNode = nodes[i, j];
                    //确定参数
                    int currentMaxEdgeCount = 0;
                    switch (currentNode.BorderCount)
                    {
                        case 0:
                            currentMaxEdgeCount = commonMaxEdgeCount;
                            break;
                        case 1:
                            currentMaxEdgeCount = borderMaxEdgeCount;
                            break;
                        default:
                            //顶点最大边数为3
                            currentMaxEdgeCount = 3;
                            break;
                    }
                    int fillCount = 0;
                    List<int> emptyList = new List<int>();
                    for (int k = 0; k < 4; k++)
                    {
                        switch (currentNode.Edges[k])
                        {
                            case EdgeType.None:
                                emptyList.Add(k);
                                break;
                            case EdgeType.Filled:
                                fillCount++;
                                break;
                        }
                    }
                    for (int k = 4; k < 8; k++)
                    {
                        if (currentNode.Edges[k] == EdgeType.Filled)
                        {
                            fillCount++;
                        }
                    }
                    int randomMin = Mathf.Max(2 - fillCount, 0);
                    int randomEdgeCount = randomMin;
                    if (emptyList.Count >= randomMin)
                    {
                        int randomMax = Mathf.Max(Convert.ToInt32(currentMaxEdgeCount * 0.5f), randomMin);
                        randomEdgeCount = UnityEngine.Random.Range(randomMin, randomMax + 1);
                    }
                    else
                    {
                        for (int k = 4; k < 8; k++)
                        {
                            if (currentNode.Edges[k] == EdgeType.None)
                            {
                                emptyList.Add(k);
                            }
                        }
                    }
                    //选边
                    List<int> chooseList = new List<int>();
                    for (int k = 0; k < randomEdgeCount; k++)
                    {
                        if (emptyList.Count <= 0)
                        {
                            break;
                        }
                        int index = UnityEngine.Random.Range(0, emptyList.Count);
                        chooseList.Add(emptyList[index]);
                        emptyList.RemoveAt(index);
                    }
                    //保存数据
                    int count = chooseList.Count;
                    for (int k = 0; k < count; k++)
                    {
                        int index = chooseList[k];
                        currentNode.Edges[index] = EdgeType.Filled;
                        //修改相邻节点状态
                        NodeIndexData neighbour = GetNeighbourIndex(i, j, index);
                        nodes[neighbour.RowIndex, neighbour.ColumnIndex].Edges[GetSymmetryEdge(index)] = EdgeType.Filled;
                    }
                }
            }
#if UNITY_EDITOR
            watch.Stop();
            Debug.LogFormat("生成: {0} ticks", watch.ElapsedTicks);
#endif
            return nodes;
        }

        private NodeIndexData GetNeighbourIndex(int currentRow, int currentColumn, int edgeIndex)
        {
            NodeIndexData data = new NodeIndexData();
            switch (edgeIndex)
            {
                case 0:
                    data.RowIndex = currentRow;
                    data.ColumnIndex = currentColumn + 1;
                    break;
                case 1:
                    data.RowIndex = currentRow + 1;
                    data.ColumnIndex = currentColumn + 1;
                    break;
                case 2:
                    data.RowIndex = currentRow + 1;
                    data.ColumnIndex = currentColumn;
                    break;
                case 3:
                    data.RowIndex = currentRow + 1;
                    data.ColumnIndex = currentColumn - 1;
                    break;
                case 4:
                    data.RowIndex = currentRow;
                    data.ColumnIndex = currentColumn - 1;
                    break;
                case 5:
                    data.RowIndex = currentRow - 1;
                    data.ColumnIndex = currentColumn - 1;
                    break;
                case 6:
                    data.RowIndex = currentRow - 1;
                    data.ColumnIndex = currentColumn;
                    break;
                case 7:
                    data.RowIndex = currentRow - 1;
                    data.ColumnIndex = currentColumn + 1;
                    break;
            }
            return data;
        }

        private int GetSymmetryEdge(int edgeIndex)
        {
            int index = 0;
            switch (edgeIndex)
            {
                case 0:
                    index = 4;
                    break;
                case 1:
                    index = 5;
                    break;
                case 2:
                    index = 6;
                    break;
                case 3:
                    index = 7;
                    break;
                case 4:
                    index = 0;
                    break;
                case 5:
                    index = 1;
                    break;
                case 6:
                    index = 2;
                    break;
                case 7:
                    index = 3;
                    break;
            }
            return index;
        }

        public bool CheckMapConnectivity(NodeData[,] mapData)
        {
#if UNITY_EDITOR
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
#endif
            int leftCount = mapData.Length;
            Stack<NodeIndexData> indexStack = new Stack<NodeIndexData>();
            NodeIndexData currentIndex = new NodeIndexData() { RowIndex = 0, ColumnIndex = 0 };
            NodeData currentNode = mapData[currentIndex.RowIndex, currentIndex.ColumnIndex];
            currentNode.LightOn = true;
            leftCount--;
            indexStack.Push(currentIndex);
            NodeIndexData neighbourIndex;
            NodeData neighbourNode;
            while (indexStack.Count > 0)
            {
                currentIndex = indexStack.Pop();
                currentNode = mapData[currentIndex.RowIndex, currentIndex.ColumnIndex];
                for (int i = 0; i < 8; i++)
                {
                    if (currentNode.Edges[i] == EdgeType.Filled)
                    {
                        neighbourIndex = GetNeighbourIndex(currentIndex.RowIndex, currentIndex.ColumnIndex, i);
                        neighbourNode = mapData[neighbourIndex.RowIndex, neighbourIndex.ColumnIndex];
                        if (!neighbourNode.LightOn)
                        {
                            neighbourNode.LightOn = true;
                            leftCount--;
                            indexStack.Push(currentIndex);
                            indexStack.Push(neighbourIndex);
                            break;
                        }
                    }
                }
            }
            bool success = leftCount <= 0;
            if (success)
            {
                //善后
                int rowCount = mapData.GetLength(0);
                int columnCount = mapData.GetLength(1);
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        mapData[i, j].LightOn = false;
                    }
                }
            }
#if UNITY_EDITOR
            watch.Stop();
            Debug.LogFormat("连通性检查: {0} ticks", watch.ElapsedTicks);
#endif
            return success;
        }

        public bool CheckMapAchievable(NodeData[,] mapData)
        {
#if UNITY_EDITOR
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
#endif
            int units = mapData.Length;
            uint maxTestNum = uint.MaxValue;
            maxTestNum >>= (32 - units);
            int rowCount = mapData.GetLength(0);
            int columnCount = mapData.GetLength(1);
            bool success = false;
            uint mask;
            bool click;
            NodeData currentNode;
            NodeIndexData neighbourIndex;
            NodeData neighbourNode;
            int lastRowIndex;
            bool allLightOn;
            for (uint i = 0; i <= maxTestNum; i++)
            {
                //初始化
                allLightOn = true;
                mask = 1;
                for (int j = 0; j < rowCount; j++)
                {
                    for (int k = 0; k < columnCount; k++)
                    {
                        mapData[j, k].LightOn = false;
                    }
                }
                //单次测试
                for (int j = 0; j < rowCount; j++)
                {
                    for (int k = 0; k < columnCount; k++)
                    {
                        click = (i & mask) > 0;
                        mask <<= 1;
                        if (click)
                        {
                            currentNode = mapData[j, k];
                            currentNode.LightOn = !currentNode.LightOn;
                            for (int m = 0; m < 8; m++)
                            {
                                if (currentNode.Edges[m] == EdgeType.Filled)
                                {
                                    neighbourIndex = GetNeighbourIndex(j, k, m);
                                    neighbourNode = mapData[neighbourIndex.RowIndex, neighbourIndex.ColumnIndex];
                                    neighbourNode.LightOn = !neighbourNode.LightOn;
                                }
                            }
                        }
                    }
                    //检查方案是否已失败
                    if (j > 0)
                    {
                        lastRowIndex = j - 1;
                        for (int k = 0; k < columnCount; k++)
                        {
                            if (!mapData[lastRowIndex, k].LightOn)
                            {
                                allLightOn = false;
                                break;
                            }
                        }
                    }
                    if (!allLightOn)
                    {
                        break;
                    }
                }
                //检查最后一行
                if (allLightOn)
                {
                    lastRowIndex = rowCount - 1;
                    for (int k = 0; k < columnCount; k++)
                    {
                        if (!mapData[lastRowIndex, k].LightOn)
                        {
                            allLightOn = false;
                            break;
                        }
                    }
                }
                if (allLightOn)
                {
                    success = true;
                    //善后
                    for (int j = 0; j < rowCount; j++)
                    {
                        for (int k = 0; k < columnCount; k++)
                        {
                            mapData[j, k].LightOn = false;
                        }
                    }
#if UNITY_EDITOR
                    char[] result = Convert.ToString(i, 2).ToCharArray();
                    Array.Reverse(result);
                    StringBuilder builder = new StringBuilder();
                    for (int j = 0; j < result.Length; j++)
                    {
                        builder.Append(result[j]);
                        if ((j + 1) % columnCount == 0)
                        {
                            builder.Append(' ');
                        }
                    }
                    Debug.LogFormat("解法: {0}", builder);
#endif
                    break;
                }
            }
#if UNITY_EDITOR
            watch.Stop();
            Debug.LogFormat("可达性检查: {0} ms", watch.ElapsedMilliseconds);
#endif
            return success;
        }

        private MapData TranslateMapTopology(NodeData[,] mapTopology, Vector2 topLeft, Vector2 bottomRight)
        {
            MapData mapData = new MapData();
            List<float> nodeList = new List<float>();
            List<int> edgeList = new List<int>();
            int rowCount = mapTopology.GetLength(0);
            int columnCount = mapTopology.GetLength(1);
            float totalHeight = topLeft.y - bottomRight.y;
            float totalWidth = bottomRight.x - topLeft.x;
            float stepX = totalWidth / (columnCount - 1);
            float stepY = totalHeight / (rowCount - 1);
            float x;
            float y = topLeft.y;
            for (int i = 0; i < rowCount; i++)
            {
                x = topLeft.x;
                for (int j = 0; j < columnCount; j++)
                {
                    NodeData nodeData = mapTopology[i, j];
                    nodeList.Add(x);
                    nodeList.Add(y);
                    for (int k = 0; k < 4; k++)
                    {
                        if (nodeData.Edges[k] == EdgeType.Filled)
                        {
                            NodeIndexData indexData = GetNeighbourIndex(i, j, k);
                            edgeList.Add(i * columnCount + j);
                            edgeList.Add(indexData.RowIndex * columnCount + indexData.ColumnIndex);
                        }
                    }
                    x += stepX;
                }
                y -= stepY;
            }
            mapData.Edges = edgeList.ToArray();
            mapData.Nodes = nodeList.ToArray();
            return mapData;
        }
    }
}
