using System.Collections.Generic;
using Assets.Game.Scripts.Enum;
using Assets.Game.Scripts.Signals;
using UnityEngine;

namespace Assets.Game.Scripts.Level
{
    public class ChainController : MonoBehaviour
    {
        [SerializeField] private float attachTweenDuration = 0.35f;

        private readonly List<RingHandler> _rings = new();

        public void Init(List<RingHandler> rings, byte[] ringColorBytes)
        {
            _rings.Clear();
            if (rings == null)
                return;

            _rings.AddRange(rings);

            for (int i = 0; i < _rings.Count; i++)
            {
                _rings[i].Init(this, (ColorType)ringColorBytes[i]);
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

            foreach (RingHandler candidateRing in bottomLeaves)
            {
                StickHandler stick = FindClosestMatchingStick(candidateRing, sticks);
                if (stick == null)
                    continue;

                _rings.Remove(candidateRing);
                candidateRing.DetachFromChainForStick();
                candidateRing.AttachToStickWithTween(stick, attachTweenDuration);
                return;
            }
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
