using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GuiGui.Konigsberg
{
    public class WindowManager : MonoBehaviour
    {
        public float FadeTime;
        public Ease FadeInEase;
        public Ease FadeOutEase;
        public Text NoticeText;
        public CanvasGroup NoticeGroup;
        public GameObject NoticeWindow;
        public Text ConfirmText;
        public CanvasGroup ConfirmGroup;
        public GameObject ConfirmWindow;
        public SettingWindowView SettingWindow;
        public static WindowManager Instance { get; private set; }
        public bool WindowOpened { get; set; }
        private Action m_NoticeOKCallback;
        private Action m_ConfirmOKCallback;
        private Action m_ConfirmCancelCallback;
        private Tweener m_ShowNoticeTweener;
        private Tweener m_HideNoticeTweener;
        private Tweener m_ShowConfirmTweener;
        private Tweener m_HideConfirmTweener;

        private void Awake()
        {
            Instance = this;
            WindowOpened = false;
        }

        public void ShowNoticeWindow(string message, Action okCallback)
        {
            if (WindowOpened)
            {
                return;
            }
            NoticeText.text = message;
            m_NoticeOKCallback = okCallback;
            NoticeGroup.alpha = 0;
            NoticeWindow.SetActive(true);
            if (m_ShowNoticeTweener != null)
            {
                m_ShowNoticeTweener.Restart();
            }
            else
            {
                m_ShowNoticeTweener = NoticeGroup.DOFade(1, FadeTime)
                    .SetEase(FadeInEase)
                    .OnComplete(() => WindowOpened = true)
                    .SetAutoKill(false);
            }
        }

        public void ShowConfirmWindow(string message, Action okCallback, Action cancelCallback)
        {
            if (WindowOpened)
            {
                return;
            }
            ConfirmText.text = message;
            m_ConfirmOKCallback = okCallback;
            m_ConfirmCancelCallback = cancelCallback;
            ConfirmGroup.alpha = 0;
            ConfirmWindow.SetActive(true);
            if (m_ShowConfirmTweener != null)
            {
                m_ShowConfirmTweener.Restart();
            }
            else
            {
                m_ShowConfirmTweener = ConfirmGroup.DOFade(1, FadeTime)
                    .SetEase(FadeInEase)
                    .OnComplete(() => WindowOpened = true)
                    .SetAutoKill(false);
            }
        }

        public void ShowSettingWindow()
        {
            if (WindowOpened)
            {
                return;
            }
            SettingWindow.ShowWindow();
        }

        public void OnNoticeOK()
        {
            if (m_NoticeOKCallback != null)
            {
                m_NoticeOKCallback();
            }
            if (m_HideNoticeTweener != null)
            {
                m_HideNoticeTweener.Restart();
            }
            else
            {
                m_HideNoticeTweener = NoticeGroup.DOFade(0, FadeTime)
                    .SetEase(FadeOutEase)
                    .OnComplete(() =>
                    {
                        NoticeWindow.SetActive(false);
                        WindowOpened = false;
                    })
                    .SetAutoKill(false);
            }
        }

        public void OnConfirmOK()
        {
            if (m_ConfirmOKCallback != null)
            {
                m_ConfirmOKCallback();
            }
            CloseConfirmWindow();
        }

        public void OnConfirmCancel()
        {
            if (m_ConfirmCancelCallback != null)
            {
                m_ConfirmCancelCallback();
            }
            CloseConfirmWindow();
        }

        private void CloseConfirmWindow()
        {
            if (m_HideConfirmTweener != null)
            {
                m_HideConfirmTweener.Restart();
            }
            else
            {
                m_HideConfirmTweener = ConfirmGroup.DOFade(0, FadeTime)
                    .SetEase(FadeOutEase)
                    .OnComplete(() =>
                    {
                        ConfirmWindow.SetActive(false);
                        WindowOpened = false;
                    })
                    .SetAutoKill(false);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}