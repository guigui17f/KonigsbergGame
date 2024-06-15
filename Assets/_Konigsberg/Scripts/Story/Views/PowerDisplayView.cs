using UnityEngine;
using UnityEngine.UI;

namespace GuiGui.Konigsberg.Story
{
    public class PowerDisplayView : MonoBehaviour
    {
        public Text PowerText;
        private I2String m_PowerTitle;

        private void Awake()
        {
            m_PowerTitle = new I2String("UI/costpower");
            StorySignalCenter.Instance.CostPowerChangedSignal.AddListener(OnCostPowerChanged);
        }
         
        private void Start()
        {
            OnCostPowerChanged(PlayerModel.Instance.TotalCostPower);
        }

        private void OnCostPowerChanged(int newCostPower)
        {
            PowerText.text = string.Format("{0} - {1:f0}", m_PowerTitle.CurrentString, newCostPower);
        }

        private void OnDestroy()
        {
            StorySignalCenter.Instance.CostPowerChangedSignal.RemoveListener(OnCostPowerChanged);
        }
    }
}