using UnityEngine;
using UnityEngine.UI;

namespace GuiGui.Konigsberg.Story
{
    public class MagicButtonView : MonoBehaviour
    {
        public Button MagicButton;
        public Color MagicOnColor;
        public Color MagicOffColor;
        public Image[] ButtonImages;
        public ColorBlock MagicOnBlock;
        public ColorBlock MagicOffBlock;
        private bool m_MagicOn = false;

        private void Awake()
        {
            StorySignalCenter.Instance.MagicModeChangedSignal.AddListener(OnMagicModeChanged);
        }

        public void OnClickMagic()
        {
            if (InputController.Instance.Enable)
            {
                StorySignalCenter.Instance.MagicModeChangedSignal.Dispatch(!m_MagicOn);
            }
        }

        private void OnMagicModeChanged(bool magicOn)
        {
            m_MagicOn = magicOn;
            for (int i = 0; i < ButtonImages.Length; i++)
            {
                ButtonImages[i].color = m_MagicOn ? MagicOnColor : MagicOffColor;
            }
            MagicButton.colors = m_MagicOn ? MagicOnBlock : MagicOffBlock;
        }

        private void OnDestroy()
        {
            StorySignalCenter.Instance.MagicModeChangedSignal.RemoveListener(OnMagicModeChanged);
        }
    }
}