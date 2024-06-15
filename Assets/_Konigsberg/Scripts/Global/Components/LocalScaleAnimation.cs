using DG.Tweening;
using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class LocalScaleAnimation : MonoBehaviour
    {
        public float ScaleDelta;
        public float Duration = 1;
        public Ease EaseType = Ease.InOutQuad;
        public float Interval;

        private void Start()
        {
            Vector3 targetLocalScale = transform.localScale * ScaleDelta;
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(Interval * 0.5f);
            sequence.Append(transform.DOScale(targetLocalScale, Duration).SetEase(EaseType));
            sequence.AppendInterval(Interval * 0.5f);
            sequence.SetLoops(-1, LoopType.Yoyo);
        }
    }
}