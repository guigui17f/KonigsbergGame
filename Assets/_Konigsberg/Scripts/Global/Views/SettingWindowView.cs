using DG.Tweening;
using GuiGui.Konigsberg.Story;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

namespace GuiGui.Konigsberg
{
    public class SettingWindowView : MonoBehaviour
    {
        public float FadeTime;
        public Ease FadeInEase;
        public Ease FadeOutEase;
        public CanvasGroup AlphaGroup;
        public Image SoundImage;
        public Image MusicImage;
        public Sprite SoundOnSprite;
        public Sprite SoundOffSprite;
        public Sprite MusicOnSprite;
        public Sprite MusicOffSprite;
        public Toggle[] LanguageToggles;
        private Tweener m_ShowTweener;
        private Tweener m_HideTweener;

        public void ShowWindow()
        {
            AlphaGroup.alpha = 0;
            SoundImage.sprite = AudioManager.Instance.SFXOn ? SoundOnSprite : SoundOffSprite;
            MusicImage.sprite = AudioManager.Instance.MusicOn ? MusicOnSprite : MusicOffSprite;
            string currentLanguageRadio = null;
            switch (GameModel.Instance.GetCurrentLanguage())
            {
                case "zh-CN":
                    currentLanguageRadio = "SimplifiedChineseRadio";
                    break;
                case "zh-TW":
                    currentLanguageRadio = "TraditionalChineseRadio";
                    break;
                default:
                    currentLanguageRadio = "EnglishRadio";
                    break;
            }
            for (int i = 0; i < LanguageToggles.Length; i++)
            {
                if (LanguageToggles[i].name == currentLanguageRadio)
                {
                    LanguageToggles[i].isOn = true;
                }
            }
            gameObject.SetActive(true);
            if (m_ShowTweener != null)
            {
                m_ShowTweener.Restart();
            }
            else
            {
                m_ShowTweener = AlphaGroup.DOFade(1, FadeTime)
                    .SetEase(FadeInEase)
                    .OnComplete(() => WindowManager.Instance.WindowOpened = true)
                    .SetAutoKill(false);
            }
        }

        public void OnClickSound()
        {
            GameSignalCenter.Instance.SwitchSFXSignal.Dispatch(!AudioManager.Instance.SFXOn);
            SoundImage.sprite = AudioManager.Instance.SFXOn ? SoundOnSprite : SoundOffSprite;
        }

        public void OnClickMusic()
        {
            GameSignalCenter.Instance.SwitchMusicSignal.Dispatch(!AudioManager.Instance.MusicOn);
            MusicImage.sprite = AudioManager.Instance.MusicOn ? MusicOnSprite : MusicOffSprite;
        }

        public void OnChooseLanguage(Toggle radio)
        {
            if (radio.isOn)
            {
                string languageCode = null;
                switch (radio.name)
                {
                    case "SimplifiedChineseRadio":
                        languageCode = "zh-CN";
                        break;
                    case "TraditionalChineseRadio":
                        languageCode = "zh-TW";
                        break;
                    default:
                        languageCode = "en";
                        break;
                }
                LocalizationManager.CurrentLanguageCode = languageCode;
                GameModel.Instance.SaveCurrentLanguage(LocalizationManager.CurrentLanguageCode);
                StorySignalCenter.Instance.SectionChangedSignal.Dispatch(PlayerModel.Instance.SectionID);
                StorySignalCenter.Instance.CostPowerChangedSignal.Dispatch(PlayerModel.Instance.TotalCostPower);
            }
        }

        public void OnClickOK()
        {
            if (m_HideTweener != null)
            {
                m_HideTweener.Restart();
            }
            else
            {
                m_HideTweener = AlphaGroup.DOFade(0, FadeTime)
                    .SetEase(FadeOutEase)
                    .OnComplete(() =>
                    {
                        WindowManager.Instance.WindowOpened = false;
                        gameObject.SetActive(false);
                    })
                    .SetAutoKill(false);
            }
        }

        public void OnClickHelp()
        {
            AlphaGroup.DOFade(0, FadeTime)
                .SetEase(FadeOutEase)
                .OnComplete(() =>
                {
                    WindowManager.Instance.WindowOpened = false;
                    gameObject.SetActive(false);
                    GameSignalCenter.Instance.StartTutorialSignal.Dispatch();
                });
        }
    }
}