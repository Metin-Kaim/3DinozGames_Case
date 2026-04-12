using System.Collections.Generic;
using Assets.Game.Scripts.Enum;
using Assets.Game.Scripts.Signals;
using DG.Tweening;
using UnityEngine;

namespace Assets.Game.Scripts.Level
{
    public class ChainController : MonoBehaviour
    {
        [SerializeField] private float attachToStickTweenDuration = 0.35f;

        [Header("Chain sway — RingContainer (simulated)")]
        [Tooltip("LevelGenerator RingContainer geçirmezse: doluysa bu, yoksa parent kullanılır.")]
        [SerializeField] private bool enableChainSway = true;
        [SerializeField] private float swayFrequencyHz = 0.95f;
        [SerializeField] private float swayAmplitudeXDeg = 1.8f;
        [SerializeField] private float swayAmplitudeYDeg = 3.2f;
        [SerializeField] private float swayAmplitudeZDeg = 4.5f;

        [Header("Chain push — touch feedback")]
        [SerializeField] private float pushPunchStrength = 0.14f;
        [SerializeField] private float pushDuration = 0.28f;
        [SerializeField] private int pushVibrato = 10;
        [SerializeField][Range(0f, 1f)] private float pushElasticity = 0.65f;
        [SerializeField] private float pushStaggerSeconds = 0.018f;

        private readonly List<RingHandler> _rings = new();

        private Transform _swayRoot;
        private Quaternion _baseSwayRootLocalRotation;

        private float _phasePrimary;
        private float _phaseY;
        private float _phaseZ;
        private float _freqMul;
        private float _ampMul;
        private Vector3 _axisMul;

        private void Awake()
        {
            ResolveSwayRoot(null);
            RollSwayRandomization();
        }

        private void ResolveSwayRoot(Transform ringContainer)
        {
            _swayRoot = ringContainer;
        }

        private void RollSwayRandomization()
        {
            _phasePrimary = Random.Range(0f, Mathf.PI * 2f);
            _phaseY = Random.Range(0f, Mathf.PI * 2f);
            _phaseZ = Random.Range(0f, Mathf.PI * 2f);
            _freqMul = Random.Range(0.82f, 1.22f);
            _ampMul = Random.Range(0.78f, 1.28f);
            _axisMul = new Vector3(Random.Range(0.85f, 1.15f), Random.Range(0.85f, 1.15f), Random.Range(0.85f, 1.15f));
        }

        public void Init(List<RingHandler> rings, byte[] ringColorBytes, Transform ringContainer = null)
        {
            _rings.Clear();
            if (rings == null)
                return;

            ResolveSwayRoot(ringContainer);

            _rings.AddRange(rings);

            for (int i = 0; i < _rings.Count; i++)
                _rings[i].Init(this, (ColorType)ringColorBytes[i]);

            if (_swayRoot != null)
                _baseSwayRootLocalRotation = _swayRoot.localRotation;
        }

        private void LateUpdate()
        {
            if (!enableChainSway || _swayRoot == null)
                return;

            if (_rings.Count == 0)
            {
                _swayRoot.localRotation = _baseSwayRootLocalRotation;
                return;
            }

            float t = Time.time;
            float w = t * swayFrequencyHz * _freqMul * (Mathf.PI * 2f) + _phasePrimary;

            const float yFreqMul = 1.07f;
            const float zFreqMul = 0.93f;

            float sx = Mathf.Sin(w);
            float sy = Mathf.Sin(w * yFreqMul + _phaseY);
            float sz = Mathf.Sin(w * zFreqMul + _phaseZ);

            var euler = new Vector3(
                sx * swayAmplitudeXDeg * _axisMul.x,
                sy * swayAmplitudeYDeg * _axisMul.y,
                sz * swayAmplitudeZDeg * _axisMul.z) * _ampMul;

            _swayRoot.localRotation = _baseSwayRootLocalRotation * Quaternion.Euler(euler);
        }

        public void PlayChainPushEffect()
        {
            if (_rings.Count == 0)
                return;

            Vector3 punch = new Vector3(pushPunchStrength, pushPunchStrength, pushPunchStrength);

            for (int i = 0; i < _rings.Count; i++)
            {
                RingHandler ring = _rings[i];
                if (ring == null)
                    continue;

                Transform tr = ring.transform;
                tr.DOKill(true);
                tr.DOPunchScale(punch, pushDuration, pushVibrato, pushElasticity)
                    .SetDelay(i * pushStaggerSeconds);
            }
        }

        public void OnRingClicked()
        {
            if (LevelSignals.Instance == null)
                return;

            IReadOnlyList<StickHandler> sticks = LevelSignals.Instance.onGetSticks?.Invoke();
            if (sticks == null || sticks.Count == 0)
                return;

            List<RingHandler> bottomLeaves = GetBottomLeafRings();
            if (bottomLeaves.Count == 0)
                return;

            PlayChainPushEffect();

            foreach (RingHandler candidateRing in bottomLeaves)
            {
                StickHandler stick = FindClosestMatchingStick(candidateRing, sticks);
                if (stick == null)
                    continue;

                _rings.Remove(candidateRing);
                candidateRing.DetachFromChainForStick();
                candidateRing.AttachToStickWithTween(stick, attachToStickTweenDuration);
                AudioSignals.Instance?.onPlaySound?.Invoke(GameSoundType.ChainMatchFound);
                return;
            }

            AudioSignals.Instance?.onPlaySound?.Invoke(GameSoundType.ChainMatchNotFound);
        }

        private StickHandler FindClosestMatchingStick(RingHandler candidateRing, IReadOnlyList<StickHandler> sticks)
        {
            StickHandler best = null;
            float bestSqr = float.PositiveInfinity;

            for (int i = 0; i < sticks.Count; i++)
            {
                StickHandler stick = sticks[i];
                if (stick == null || !stick.CanAcceptRing())
                    continue;
                if (stick.ColorType != candidateRing.ColorType)
                    continue;

                float sqr = (stick.transform.position - candidateRing.transform.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = stick;
                }
            }

            return best;
        }

        private List<RingHandler> GetBottomLeafRings()
        {
            var leaves = new List<RingHandler>();
            foreach (RingHandler r in _rings)
            {
                if (r.LowerRings != null && r.LowerRings.Count != 0)
                    continue;
                leaves.Add(r);
            }

            return leaves;
        }
    }
}
