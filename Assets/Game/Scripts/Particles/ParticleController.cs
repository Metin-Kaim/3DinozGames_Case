using System.Collections.Generic;
using Assets.Game.Scripts.Signals;
using UnityEngine;

namespace Assets.Game.Scripts.Particles
{
    public class ParticleController : MonoBehaviour
    {
        [Header("Win (havuz dışı)")]
        [SerializeField] private ParticleSystem winParticle;
        [SerializeField] private Transform winParticleSpawnPoint;

        [Header("Ring (havuz)")]
        [SerializeField] private ParticleHandler ringParticlePrefab;
        [SerializeField] private Transform ringPoolRoot;
        [SerializeField] private int ringPoolInitialSize = 8;
        [SerializeField] private float ringParticleYOffset = 0.12f;

        private readonly Queue<ParticleHandler> _ringPool = new();

        private void Awake()
        {
            if (ringPoolRoot == null)
            {
                var go = new GameObject("RingParticlePool");
                go.transform.SetParent(transform, false);
                ringPoolRoot = go.transform;
            }

            if (ringParticlePrefab == null)
                return;

            for (int i = 0; i < ringPoolInitialSize; i++)
            {
                ParticleHandler handler = Instantiate(ringParticlePrefab, ringPoolRoot);
                handler.gameObject.SetActive(false);
                _ringPool.Enqueue(handler);
            }
        }

        private void OnEnable()
        {
            if (ParticleSignals.Instance != null)
            {
                ParticleSignals.Instance.onSpawnRingParticle += SpawnRingParticleAt;
                ParticleSignals.Instance.onRingParticleReturned += OnRingParticleReturned;
            }

            if (GameSignals.Instance != null)
                GameSignals.Instance.onGameEnded += OnGameEnded;
        }

        private void OnDisable()
        {
            if (ParticleSignals.Instance != null)
            {
                ParticleSignals.Instance.onSpawnRingParticle -= SpawnRingParticleAt;
                ParticleSignals.Instance.onRingParticleReturned -= OnRingParticleReturned;
            }

            if (GameSignals.Instance != null)
                GameSignals.Instance.onGameEnded -= OnGameEnded;
        }

        private void OnGameEnded(bool isWin, float delay)
        {
            if (!isWin || winParticle == null)
                return;

            // Instantiate the win particle if it doesn't exist already
            ParticleSystem winParticleInstance = Instantiate(winParticle);

            if (winParticleSpawnPoint != null)
                winParticleInstance.transform.position = winParticleSpawnPoint.position;

            winParticleInstance.Play();
        }


        private void SpawnRingParticleAt(Vector3 worldRingCenter)
        {
            if (ringParticlePrefab == null)
                return;

            ParticleHandler handler = RentRingHandler();
            Vector3 pos = worldRingCenter + Vector3.up * ringParticleYOffset;
            handler.Play(pos);
        }

        private ParticleHandler RentRingHandler()
        {
            if (_ringPool.Count > 0)
                return _ringPool.Dequeue();

            return Instantiate(ringParticlePrefab, ringPoolRoot);
        }

        private void OnRingParticleReturned(ParticleHandler handler)
        {
            _ringPool.Enqueue(handler);
        }
    }
}
