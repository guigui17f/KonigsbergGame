using UnityEngine;
using UnityEngine.UI;

namespace GuiGui.Konigsberg.Story
{
    public class SectionDisplayView : MonoBehaviour
    {
        public Text SectionText;
        private I2String m_SectionTitle;

        private void Awake()
        {
            m_SectionTitle = new I2String("UI/section");
            StorySignalCenter.Instance.SectionChangedSignal.AddListener(OnSectionChanged);
        }

        private void Start()
        {
            OnSectionChanged(PlayerModel.Instance.SectionID);
        }

        private void OnSectionChanged(int sectionId)
        {
            SectionText.text = string.Format("{0} - {1:f0}", m_SectionTitle.CurrentString, sectionId + 1);
        }

        private void OnDestroy()
        {
            StorySignalCenter.Instance.SectionChangedSignal.RemoveListener(OnSectionChanged);
        }
    }
}
