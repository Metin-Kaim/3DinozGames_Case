using System.Collections;
using Assets.Game.Scripts.Signals;
using UnityEngine;

namespace Assets.Game.Scripts.Particles
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleHandler : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleSystem;

        public void Play(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            gameObject.SetActive(true);
            particleSystem.Clear(true);
            particleSystem.Play();
        }

        private void OnDisable()
        {
            ParticleSignals.Instance?.onRingParticleReturned?.Invoke(this);
        }
    }
}
