using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GuiGui.Konigsberg
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource MusicSource;
        public AudioSource SFXSource;
        public AudioClip[] MusicClips;
        public AudioClip ClickUI;
        public AudioClip ClickNode;
        public AudioClip ClickMagic;
        public AudioClip Success;
        public static AudioManager Instance { get; private set; }
        public bool MusicOn { get; private set; }
        public bool SFXOn { get; private set; }
        private const string MUSIC_STATE_KEY = "MusicState";
        private const string SFX_STATE_KEY = "SFXState";
        private List<AudioClip> m_CurrentMusicList;
        private WaitForSeconds m_CheckMusicWait;

        private void Awake()
        {
            Instance = this;
            m_CurrentMusicList = new List<AudioClip>();
            m_CheckMusicWait = new WaitForSeconds(1);
            if (UserDataService.DataPersister.Exists(MUSIC_STATE_KEY))
            {
                MusicOn = UserDataService.DataPersister.Load<bool>(MUSIC_STATE_KEY);
            }
            else
            {
                MusicOn = true;
            }
            if (UserDataService.DataPersister.Exists(SFX_STATE_KEY))
            {
                SFXOn = UserDataService.DataPersister.Load<bool>(SFX_STATE_KEY);
            }
            else
            {
                SFXOn = true;
            }
            GameSignalCenter.Instance.SwitchMusicSignal.AddListener(OnSwitchMusic);
            GameSignalCenter.Instance.SwitchSFXSignal.AddListener(OnSwitchSFX);
            GameSignalCenter.Instance.PlaySFXSignal.AddListener(OnPlaySFX);
            ChooseNextMusic();
        }

        private void Start()
        {
            if (MusicOn)
            {
                MusicSource.Play();
                StartCoroutine(CoCheckMusicFinish());
            }
        }

        private void ChooseNextMusic()
        {
            if (m_CurrentMusicList.Count <= 0)
            {
                m_CurrentMusicList.AddRange(MusicClips);
            }
            int index = UnityEngine.Random.Range(0, m_CurrentMusicList.Count);
            MusicSource.clip = m_CurrentMusicList[index];
            m_CurrentMusicList.RemoveAt(index);
        }

        private void OnSwitchMusic(bool switchOn)
        {
            MusicOn = switchOn;
            if (MusicOn)
            {
                MusicSource.Play();
                StartCoroutine(CoCheckMusicFinish());
            }
            else
            {
                MusicSource.Pause();
            }
            UserDataService.DataPersister.Save(MUSIC_STATE_KEY, MusicOn);
        }

        private void OnSwitchSFX(bool switchOn)
        {
            SFXOn = switchOn;
            UserDataService.DataPersister.Save(SFX_STATE_KEY, SFXOn);
        }

        private void OnPlaySFX(SFXType sfxType)
        {
            if (SFXOn)
            {
                switch (sfxType)
                {
                    case SFXType.NodeClick:
                        SFXSource.PlayOneShot(ClickNode);
                        break;
                    case SFXType.MagicClick:
                        SFXSource.PlayOneShot(ClickMagic);
                        break;
                    case SFXType.Success:
                        SFXSource.PlayOneShot(Success, 1.2f);
                        break;
                    default:
                        SFXSource.PlayOneShot(ClickUI);
                        break;
                }
            }
        }

        private IEnumerator CoCheckMusicFinish()
        {
            while (MusicOn)
            {
                if (!MusicSource.isPlaying)
                {
                    ChooseNextMusic();
                    MusicSource.Play();
                }
                yield return m_CheckMusicWait;
            }
        }

        private void OnDestroy()
        {
            GameSignalCenter.Instance.SwitchMusicSignal.RemoveListener(OnSwitchMusic);
            GameSignalCenter.Instance.SwitchSFXSignal.RemoveListener(OnSwitchSFX);
            GameSignalCenter.Instance.PlaySFXSignal.RemoveListener(OnPlaySFX);
            Instance = null;
        }
    }
}