using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MainGame
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager SharedInstance;

        [SerializeField] private AudioMixerGroup mainMixerGroup;
        [SerializeField] private Sound[] sounds;

        private void Awake()
        {
            if (SharedInstance == null)
                SharedInstance = this;
            else
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            foreach (Sound s in sounds)
            {
                GameObject obj = new GameObject();
                obj.transform.parent = transform;
                obj.name = s.name;
                s.source = obj.AddComponent<AudioSource>();
                s.source.outputAudioMixerGroup = mainMixerGroup;
                s.source.clip = s.clip;

                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
        }

        public void PlayTheme(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
                return;
            if (!s.source.isPlaying)
                s.source.Play();
        }

        public void Play(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
                return;
            s.source.Play();
        }

        public void Stop(string name)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
                return;
            s.source.Stop();
        }
    }
}
