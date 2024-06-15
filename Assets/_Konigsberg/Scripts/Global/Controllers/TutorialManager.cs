using DG.Tweening;
using HedgehogTeam.EasyTouch;
using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class TutorialManager : MonoBehaviour
    {
        public GameObject TutorialNotice0;
        public CanvasGroup TutorialCanvas0;
        public GameObject TutorialNotice1;
        public CanvasGroup TutorialCanvas1;
        public Ease FadeInEase;
        public Ease FadeOutEase;
        public float FadeTime;
        private const string TUTORIAL_KEY = "TutorialShown";
        private int m_ClickCount;

        private void Awake()
        {
            GameSignalCenter.Instance.StartTutorialSignal.AddListener(StartTutorial);
        }

        private void Start()
        {
            if (!PlayerPrefs.HasKey(TUTORIAL_KEY))
            {
                PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
                StartTutorial();
            }
        }

        private void StartTutorial()
        {
            InputController.Instance.SetEnable(false);
            m_ClickCount = 0;
            TutorialCanvas0.alpha = 0;
            TutorialNotice0.SetActive(true);
            TutorialCanvas0.DOFade(1, FadeTime)
                .SetEase(FadeInEase)
                .OnComplete(() =>
                {
                    EasyTouch.On_TouchUp += OnTouchUp;
                    InputController.Instance.SetEnable(true);
                });
        }

        private void OnTouchUp(Gesture gesture)
        {
            InputController.Instance.SetEnable(false);
            switch (m_ClickCount)
            {
                case 0:
                    TutorialCanvas0.DOFade(0, FadeTime)
                        .SetEase(FadeOutEase)
                        .OnComplete(() =>
                        {
                            TutorialNotice0.SetActive(false);
                            TutorialCanvas1.alpha = 0;
                            TutorialNotice1.SetActive(true);
                            TutorialCanvas1.DOFade(1, FadeTime)
                            .SetEase(FadeInEase)
                            .OnComplete(() =>
                            {
                                InputController.Instance.SetEnable(true);
                            });
                        });
                    break;
                default:
                    TutorialCanvas1.DOFade(0, FadeTime)
                        .SetEase(FadeOutEase)
                        .OnComplete(() =>
                        {
                            TutorialNotice1.SetActive(false);
                            EasyTouch.On_TouchUp -= OnTouchUp;
                            InputController.Instance.SetEnable(true);
                        });
                    break;
            }
            m_ClickCount++;
        }

        private void OnDestroy()
        {
            GameSignalCenter.Instance.StartTutorialSignal.RemoveListener(StartTutorial);
        }
    }
}