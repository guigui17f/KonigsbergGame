using DG.Tweening;
using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class LocalMoveAnimation : MonoBehaviour
    {
        public Vector3 TargetLocalPosition;
        public float Duration = 1;
        public Ease EaseType = Ease.InOutQuad;
        public float Interval;

        private void Start()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(Interval * 0.5f);
            sequence.Append(transform.DOLocalMove(TargetLocalPosition, Duration).SetEase(EaseType));
            sequence.AppendInterval(Interval * 0.5f);
            sequence.SetLoops(-1, LoopType.Yoyo);
        }
    }
}