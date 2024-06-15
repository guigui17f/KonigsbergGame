using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GuiGui.Konigsberg
{
    public class MaskManager : MonoBehaviour
    {
        public Graphic Mask;
        public float FadeTime;
        public float BlockTime;
        public static MaskManager Instance { get; private set; }
        private WaitForSeconds fadeWait;
        private WaitForSeconds blockWait;

        private void Awake()
        {
            Instance = this;
            fadeWait = new WaitForSeconds(FadeTime);
            blockWait = new WaitForSeconds(BlockTime);
        }

        public void ShowMask(Action maskCallback, Action finishCallback)
        {
            StartCoroutine(CoShowMask(maskCallback, finishCallback));
        }

        private IEnumerator CoShowMask(Action maskCallback, Action finishCallback)
        {
            Mask.CrossFadeAlpha(0, 0, true);
            Mask.gameObject.SetActive(true);
            Mask.CrossFadeAlpha(1, FadeTime, false);
            yield return fadeWait;
            if (maskCallback != null)
            {
                maskCallback();
            }
            yield return blockWait;
            Resources.UnloadUnusedAssets();
            GC.Collect();
            Mask.CrossFadeAlpha(0, FadeTime, false);
            yield return fadeWait;
            Mask.gameObject.SetActive(false);
            if (finishCallback != null)
            {
                finishCallback();
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}