using Assets.Scripts.Notifications;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Scripts.Audio
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField]
        private List<AudioClip> musicTracks;

        [SerializeField]
        private List<AudioClip> ambientTracks;

        [SerializeField]
        private AudioMixer mixer;

        private AudioSource source;

        private bool activeIsMusic = true;
        private int musicTrackPosition;
        private int ambientTrackPosition;

        private static MusicManager _instance = null;
        public static MusicManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<MusicManager>();
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            source = GetComponent<AudioSource>();
            source.clip = musicTracks[0];
            source.Play();
        }

        private void OnEnable()
        {
            this.AddObserver(OnFloorChange, GameManager.SubmitTurnNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnFloorChange, GameManager.SubmitTurnNotification);
        }

        private void Update()
        {
            if (activeIsMusic && !source.isPlaying)
            {
                activeIsMusic = false;

                ambientTrackPosition++;
                ambientTrackPosition %= ambientTracks.Count;
                source.clip = ambientTracks[ambientTrackPosition];
                source.Play();
            }
        }

        private void OnFloorChange(object sender, object args)
        {
            StartCoroutine(ChangeTracks());
        }

        public IEnumerator ChangeTracks()
        {
            yield return StartCoroutine(FadeMixerGroup.StartFade(mixer, "MusicVolume", 3f, -80f));

            musicTrackPosition++;
            musicTrackPosition %= musicTracks.Count;
            source.clip = musicTracks[musicTrackPosition];
            source.Play();
            activeIsMusic = true;

            yield return StartCoroutine(FadeMixerGroup.StartFade(mixer, "MusicVolume", 1f, 0f));
        }
    }

    public static class FadeMixerGroup
    {
        public static IEnumerator StartFade(AudioMixer mixer, string param, float duration, float target)
        {
            var time = 0f;
            mixer.GetFloat(param, out var current);

            while (time < duration)
            {
                time += Time.deltaTime;

                var val = Mathf.Lerp(current, target, time / duration);
                mixer.SetFloat(param, val);

                yield return null;
            }
        }
    }
}
