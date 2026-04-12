using System;
using System.Collections.Generic;
using Assets.Game.Scripts.Enum;
using Assets.Game.Scripts.Signals;
using UnityEngine;

namespace Assets.Game.Scripts.Audio
{
    public class AudioController : MonoBehaviour
    {
        [Serializable]
        private struct GameSoundClipEntry
        {
            public GameSoundType soundType;
            public AudioClip clip;
        }

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private GameSoundClipEntry[] soundEntries;

        private readonly Dictionary<GameSoundType, AudioClip> _clipsBySound = new();

        private void Awake()
        {
            _clipsBySound.Clear();
            if (soundEntries == null)
                return;

            for (int i = 0; i < soundEntries.Length; i++)
            {
                GameSoundClipEntry entry = soundEntries[i];
                if (entry.clip == null)
                    continue;

                _clipsBySound[entry.soundType] = entry.clip;
            }
        }

        private void OnEnable()
        {
            if (AudioSignals.Instance != null)
                AudioSignals.Instance.onPlaySound += PlayOneShot;
        }

        private void OnDisable()
        {
            if (AudioSignals.Instance != null)
                AudioSignals.Instance.onPlaySound -= PlayOneShot;
        }

        private void PlayOneShot(GameSoundType soundType)
        {
            if (!_clipsBySound.TryGetValue(soundType, out AudioClip clip))
                return;

            audioSource.PlayOneShot(clip);
        }
    }
}
