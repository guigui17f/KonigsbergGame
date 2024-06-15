using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuiGui.Konigsberg.Story
{
    public class StorySceneManager : MonoBehaviour
    {
        public Transform NodeRoot;
        public Transform EdgeRoot;
        public Transform TopLeft;
        public Transform BottomRight;
        public float EndShowDuration;

        private int currentSectionId;
        private bool[] currentSectionState;
        private FastPool nodePool;
        private FastPool edgePool;
        private List<NodeView> currentNodeList;
        private List<EdgeView> currentEdgeList;
        private int actionCount = 0;
        private float realTimeSinceSectionStart;
        private WaitForSeconds m_EndShowWait;

        private void Awake()
        {
            m_EndShowWait = new WaitForSeconds(EndShowDuration);
            nodePool = FastPoolManager.GetPool(0, null, false);
            edgePool = FastPoolManager.GetPool(1, null, false);
            GameSignalCenter.Instance.NodeStateChangingSignal.AddListener(OnChangingNode);
            GameSignalCenter.Instance.NodeStateChangedSignal.AddListener(OnChangedNode);
            StorySignalCenter.Instance.MagicCostingSignal.AddListener(OnMagicCosting);
        }

        private void Start()
        {
            InitScene(PlayerModel.Instance.SectionID);
        }

        private void InitScene(int sectionId)
        {
            InputController.Instance.SetEnable(false);
            MapData mapData = GameModel.Instance.GetCurrentMapData();
            if (mapData == null)
            {
                mapData = MapDataGenerator.Instance.CreateNewData(sectionId, TopLeft.localPosition, BottomRight.localPosition);
                MapCreater.Instance.CreateMap(mapData, NodeRoot, EdgeRoot, out currentNodeList, out currentEdgeList);
                currentSectionState = new bool[currentNodeList.Count];
                GameModel.Instance.SaveCurrentMapData(mapData);
                GameModel.Instance.SaveCurrentSectionState(currentSectionState);
            }
            else
            {
                currentSectionState = GameModel.Instance.GetCurrentSectionState();
                MapCreater.Instance.LoadMap(mapData, currentSectionState, NodeRoot, EdgeRoot, out currentNodeList, out currentEdgeList);
            }
            currentSectionId = sectionId;
            #region 统计数据处理
            actionCount = 0;
            realTimeSinceSectionStart = Time.realtimeSinceStartup;
            #endregion
            InputController.Instance.SetEnable(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                WindowManager.Instance.ShowConfirmWindow(new I2String("Confirm/quit").CurrentString, () => Application.Quit(), null);
            }
        }

        private void OnChangingNode(int id, bool lightOn)
        {
            currentSectionState[id] = lightOn;
        }

        private void OnChangedNode()
        {
            actionCount++;
            bool passed = true;
            for (int i = 0; i < currentSectionState.Length; i++)
            {
                if (!currentSectionState[i])
                {
                    passed = false;
                    break;
                }
            }
            if (passed)
            {
                GameSignalCenter.Instance.PlaySFXSignal.Dispatch(SFXType.Success);
                StartCoroutine(CoLoadSection(currentSectionId + 1));
            }
            else
            {
                GameModel.Instance.SaveCurrentSectionState(currentSectionState);
                InputController.Instance.SetEnable(true);
            }
        }

        private IEnumerator CoLoadSection(int sectionId)
        {
            yield return m_EndShowWait;
            SetSection(sectionId);
        }


        private void OnMagicCosting()
        {
            #region 统计数据处理
            //DataAnalyticService.SendMagicEvent(currentSectionId, actionCount, Time.realtimeSinceStartup - realTimeSinceSectionStart, PlayerModel.Instance.Power);
            #endregion
        }

        private void SetSection(int sectionId)
        {
            if (sectionId == currentSectionId || sectionId < 0)
            {
                return;
            }
            else
            {
                InputController.Instance.SetEnable(false);
                GameModel.Instance.CleanCurrentMapData();
                GameModel.Instance.CleanCurrentSectionState();
                #region 统计数据处理
                if (sectionId > 0 && actionCount > 0)
                {
                    //DataAnalyticService.SendSectionPassedEvent(currentSectionId, actionCount, Time.realtimeSinceStartup - realTimeSinceSectionStart);
                }
                #endregion
                MaskManager.Instance.ShowMask(() =>
                {
                    PlayerModel.Instance.ClearSectionCostPower();
                    PlayerModel.Instance.SetPlayerSection(sectionId);
                    PlayerModel.Instance.SavePlayerStatus();
                    int nodeCount = currentNodeList.Count;
                    for (int i = 0; i < nodeCount; i++)
                    {
                        nodePool.FastDestroy(currentNodeList[i]);
                    }
                    int edgeCount = currentEdgeList.Count;
                    for (int i = 0; i < edgeCount; i++)
                    {
                        edgePool.FastDestroy(currentEdgeList[i]);
                    }
                    MapData mapData = MapDataGenerator.Instance.CreateNewData(sectionId, TopLeft.localPosition, BottomRight.localPosition);
                    MapCreater.Instance.CreateMap(mapData, NodeRoot, EdgeRoot, out currentNodeList, out currentEdgeList);
                    currentSectionState = new bool[currentNodeList.Count];
                    GameModel.Instance.SaveCurrentMapData(mapData);
                    GameModel.Instance.SaveCurrentSectionState(currentSectionState);
                    StorySignalCenter.Instance.MagicModeChangedSignal.Dispatch(false);
                    currentSectionId = sectionId;
                }, () =>
                {
                    #region 统计数据处理
                    actionCount = 0;
                    realTimeSinceSectionStart = Time.realtimeSinceStartup;
                    #endregion
                    InputController.Instance.SetEnable(true);
                });
            }
        }

        public void ResetCurrentSection()
        {
            if (InputController.Instance.Enable)
            {
                WindowManager.Instance.ShowConfirmWindow(new I2String("Confirm/reset").CurrentString, () =>
                {
                    InputController.Instance.SetEnable(false);
                    MaskManager.Instance.ShowMask(() =>
                    {
                        GameSignalCenter.Instance.ResetSectionSignal.Dispatch();
                        PlayerModel.Instance.ResetSectionCostPower();
                        PlayerModel.Instance.SavePlayerStatus();
                        currentSectionState = new bool[currentNodeList.Count];
                        GameModel.Instance.SaveCurrentSectionState(currentSectionState);
                        StorySignalCenter.Instance.MagicModeChangedSignal.Dispatch(false);
                        #region 统计数据处理
                        if (actionCount > 0)
                        {
                            //DataAnalyticService.SendSectionResetEvent(currentSectionId, actionCount, Time.realtimeSinceStartup - realTimeSinceSectionStart);
                        }
                        actionCount = 0;
                        realTimeSinceSectionStart = Time.realtimeSinceStartup;
                        #endregion
                    }, () =>
                    {
                        InputController.Instance.SetEnable(true);
                    });
                }, null);
            }
        }

        public void StartNewGame()
        {
            if (InputController.Instance.Enable)
            {
                WindowManager.Instance.ShowConfirmWindow(new I2String("Confirm/newgame").CurrentString, () =>
                {
                    InputController.Instance.SetEnable(false);
                    #region 统计数据处理
                    //DataAnalyticService.SendNewGameEvent(PlayerModel.Instance.SectionID, PlayerModel.Instance.Power);
                    #endregion
                    PlayerModel.Instance.ResetPlayerStatus();
                    GameModel.Instance.CleanCurrentMapData();
                    GameModel.Instance.CleanCurrentSectionState();
                    MaskManager.Instance.ShowMask(() =>
                    {
                        StorySignalCenter.Instance.SectionChangedSignal.Dispatch(PlayerModel.Instance.SectionID);
                        StorySignalCenter.Instance.CostPowerChangedSignal.Dispatch(PlayerModel.Instance.TotalCostPower);
                        int nodeCount = currentNodeList.Count;
                        for (int i = 0; i < nodeCount; i++)
                        {
                            nodePool.FastDestroy(currentNodeList[i]);
                        }
                        int edgeCount = currentEdgeList.Count;
                        for (int i = 0; i < edgeCount; i++)
                        {
                            edgePool.FastDestroy(currentEdgeList[i]);
                        }
                        MapData mapData = MapDataGenerator.Instance.CreateNewData(0, TopLeft.localPosition, BottomRight.localPosition);
                        MapCreater.Instance.CreateMap(mapData, NodeRoot, EdgeRoot, out currentNodeList, out currentEdgeList);
                        currentSectionState = new bool[currentNodeList.Count];
                        GameModel.Instance.SaveCurrentMapData(mapData);
                        GameModel.Instance.SaveCurrentSectionState(currentSectionState);
                        StorySignalCenter.Instance.MagicModeChangedSignal.Dispatch(false);
                        currentSectionId = 0;
                    }, () =>
                    {
                        #region 统计数据处理
                        actionCount = 0;
                        realTimeSinceSectionStart = Time.realtimeSinceStartup;
                        #endregion
                        InputController.Instance.SetEnable(true);
                    });
                }, null);
            }
        }

        private void OnApplicationQuit()
        {
            //DataAnalyticService.SendApplicationQuitEvent(currentSectionId, actionCount, Time.realtimeSinceStartup - realTimeSinceSectionStart, Time.realtimeSinceStartup);
        }

        private void OnDestroy()
        {
            GameSignalCenter.Instance.NodeStateChangingSignal.RemoveListener(OnChangingNode);
            GameSignalCenter.Instance.NodeStateChangedSignal.RemoveListener(OnChangedNode);
            StorySignalCenter.Instance.MagicCostingSignal.RemoveListener(OnMagicCosting);
        }
    }
}