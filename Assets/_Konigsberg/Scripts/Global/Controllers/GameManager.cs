using I2.Loc;
using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 30;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_EDITOR
            Debug.EnableDebug = true;
#else
            Debug.EnableDebug = false;
#endif
            string language = GameModel.Instance.GetCurrentLanguage();
            if (language != null)
            {
                LocalizationManager.CurrentLanguageCode = language;
            }
            else
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.Chinese:
                    case SystemLanguage.ChineseSimplified:
                        LocalizationManager.CurrentLanguageCode = "zh-CN";
                        break;
                    case SystemLanguage.ChineseTraditional:
                        LocalizationManager.CurrentLanguageCode = "zh-TW";
                        break;
                    default:
                        LocalizationManager.CurrentLanguageCode = "en";
                        break;
                }
                GameModel.Instance.SaveCurrentLanguage(LocalizationManager.CurrentLanguageCode);
            }
        }
    }
}
