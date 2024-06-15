using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GuiGui.Konigsberg
{
    public class CaptureButtonView : MonoBehaviour
    {
        public Button CaptureButton;
        public Image CaptureImage;
        public Color EnableColor;
        public Color DisableColor;
        public CaptureAndSave CaptureHelper;
        private WaitForSeconds m_CaptureDelay;
        private WaitForSeconds m_NoticeDelay;

        private void Awake()
        {
            m_CaptureDelay = new WaitForSeconds(0.15f);
            m_NoticeDelay = new WaitForSeconds(0.25f);
        }

        public void OnClickCapture()
        {
            if (InputController.Instance.Enable)
            {
                CaptureButton.interactable = false;
                CaptureImage.color = DisableColor;
                StartCoroutine(CoCapture());
            }
        }

        private IEnumerator CoCapture()
        {
            yield return m_CaptureDelay;
            CaptureHelper.CaptureAndSaveToAlbum(ImageType.JPG);
            yield return m_NoticeDelay;
            WindowManager.Instance.ShowNoticeWindow(new I2String("Notice/captureSaved").CurrentString, () =>
            {
                CaptureImage.color = EnableColor;
                CaptureButton.interactable = true;
            });
        }
    }
}