using Assets.Game.Scripts.Particles;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Game.Scripts.Signals
{
    public class ParticleSignals : MonoBehaviour
    {
        public static ParticleSignals Instance;

        /// <summary>World-space ring center; ParticleController üst offset uygular.</summary>
        public UnityAction<Vector3> onSpawnRingParticle;

        /// <summary>Ring partikülü devre dışı kalınca havuza iade (ParticleHandler OnDisable).</summary>
        public UnityAction<ParticleHandler> onRingParticleReturned;

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
