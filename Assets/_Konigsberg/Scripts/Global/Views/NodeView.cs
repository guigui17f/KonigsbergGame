using DG.Tweening;
using GuiGui.Konigsberg.Story;
using HedgehogTeam.EasyTouch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GuiGui.Konigsberg
{
    public class NodeView : MonoBehaviour, IFastPoolItem
    {
        public float ExpandDuration;
        public Ease ExpandEaseType;
        public RectTransform EnableRect;
        public Image EnableImage;
        public RectTransform DisableRect;
        public Image DisableImage;
        public Vector2 EndSizeDelta;
        public int Id;
        public bool LightOn;
        public Color EnableColor;
        public Color DisableColor;
        public Color PressColor;
        public float PressFadeDuration;
        public float PressFadeDelay;
        public List<EdgeView> EdgeList { get; private set; }
        private WaitForSeconds m_SendChangedDelay;
        private bool m_MagicOn = false;

        private void Awake()
        {
            m_SendChangedDelay = new WaitForSeconds(ExpandDuration);
            OnFastInstantiate();
        }

        public void OnFastInstantiate()
        {
            GameSignalCenter.Instance.ResetSectionSignal.AddListener(OnResetMap);
            StorySignalCenter.Instance.MagicModeChangedSignal.AddListener(OnMagicModeChanged);
            EasyTouch.On_SimpleTap += OnClickNode;
        }

        public void RegisterEdge(EdgeView edge)
        {
            if (null == EdgeList)
            {
                EdgeList = new List<EdgeView>();
            }
            EdgeList.Add(edge);
        }

        public void SetNodeData(int nodeIndex, bool lightOn)
        {
            Id = nodeIndex;
            LightOn = lightOn;
            if (lightOn)
            {
                EnableRect.sizeDelta = EndSizeDelta;
                EnableRect.SetAsLastSibling();
                DisableRect.sizeDelta = Vector2.zero;
            }
            else
            {
                DisableRect.sizeDelta = EndSizeDelta;
                DisableRect.SetAsLastSibling();
                EnableRect.sizeDelta = Vector2.zero;
            }
        }

        private void OnClickNode(Gesture gesture)
        {
            if (gesture.pickedObject == gameObject)
            {
                InputController.Instance.SetEnable(false);
                //Sequence sequence = DOTween.Sequence();
                //if (LightOn)
                //{
                //    sequence.Append(EnableImage.DOColor(PressColor, PressFadeDuration).SetEase(Ease.Linear));
                //    sequence.Append(EnableImage.DOColor(EnableColor, PressFadeDuration).SetEase(Ease.Linear));
                //    sequence.AppendInterval(PressFadeDelay);
                //    sequence.AppendCallback(StartChange);
                //}
                //else
                //{
                //    sequence.Append(DisableImage.DOColor(PressColor, PressFadeDuration).SetEase(Ease.Linear));
                //    sequence.Append(DisableImage.DOColor(DisableColor, PressFadeDuration).SetEase(Ease.Linear));
                //    sequence.AppendInterval(PressFadeDelay);
                //    sequence.AppendCallback(StartChange);
                //}
                StartChange();
            }
        }

        private void StartChange()
        {
            PlayChangeAnimation();
            if (m_MagicOn)
            {
                StorySignalCenter.Instance.MagicCostingSignal.Dispatch();
                PlayerModel.Instance.ChangePlayerCostPower(ProjectConst.MAGIC_COST);
                PlayerModel.Instance.SavePlayerStatus();
                StorySignalCenter.Instance.MagicModeChangedSignal.Dispatch(false);
                GameSignalCenter.Instance.PlaySFXSignal.Dispatch(SFXType.MagicClick);
            }
            else
            {
                EdgeList.ForEach(edge =>
                {
                    edge.GetNeighbourNode(this).PlayChangeAnimation();
                });
                GameSignalCenter.Instance.PlaySFXSignal.Dispatch(SFXType.NodeClick);
            }
            StartCoroutine(CoSendChangedSignal(m_SendChangedDelay));
        }

        private IEnumerator CoSendChangedSignal(WaitForSeconds delay)
        {
            yield return delay;
            GameSignalCenter.Instance.NodeStateChangedSignal.Dispatch();
        }

        public void PlayChangeAnimation()
        {
            LightOn = !LightOn;
            RectTransform showRect;
            RectTransform hideRect;
            if (LightOn)
            {
                EnableRect.sizeDelta = Vector2.zero;
                EnableRect.SetAsLastSibling();
                showRect = EnableRect;
                hideRect = DisableRect;
            }
            else
            {
                DisableRect.sizeDelta = Vector2.zero;
                DisableRect.SetAsLastSibling();
                showRect = DisableRect;
                hideRect = EnableRect;
            }
            showRect.DOSizeDelta(EndSizeDelta, ExpandDuration)
                .OnComplete(() =>
                {
                    hideRect.sizeDelta = Vector2.zero;
                }).SetEase(ExpandEaseType);
            GameSignalCenter.Instance.NodeStateChangingSignal.Dispatch(Id, LightOn);
        }

        private void OnResetMap()
        {
            SetNodeData(Id, false);
        }

        private void OnMagicModeChanged(bool magicOn)
        {
            m_MagicOn = magicOn;
        }

        public void OnFastDestroy()
        {
            GameSignalCenter.Instance.ResetSectionSignal.RemoveListener(OnResetMap);
            StorySignalCenter.Instance.MagicModeChangedSignal.RemoveListener(OnMagicModeChanged);
            EasyTouch.On_SimpleTap -= OnClickNode;
            if (EdgeList != null)
            {
                EdgeList.Clear();
            }
            m_MagicOn = false;
        }

        private void OnDestroy()
        {
            OnFastDestroy();
        }
    }
}
