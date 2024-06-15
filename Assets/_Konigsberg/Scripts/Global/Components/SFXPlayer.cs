using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class SFXPlayer : MonoBehaviour
    {
        public SFXType PlayType;

        public void OnClickTarget()
        {
            GameSignalCenter.Instance.PlaySFXSignal.Dispatch(PlayType);
        }
    }
}