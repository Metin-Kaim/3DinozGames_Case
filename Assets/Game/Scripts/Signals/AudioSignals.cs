using Assets.Game.Scripts.Enum;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Game.Scripts.Signals
{
    public class AudioSignals : MonoBehaviour
    {
        public static AudioSignals Instance;

        public UnityAction<GameSoundType> onPlaySound;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
