using System.Collections.Generic;
using Assets.Game.Scripts.Enum;
using Assets.Game.Scripts.Signals;
using UnityEngine;

namespace Assets.Game.Scripts.Level
{
    public class ChainController : MonoBehaviour
    {
        private const float BottomYEpsilon = 0.001f;

        [SerializeField] private float attachTweenDuration = 0.35f;

        private readonly List<ChainRingHandler> _rings = new();

        public List<ChainRingHandler> Rings => _rings;

        public void Init(List<ChainRingHandler> rings)
        {
            _rings.Clear();
            if (rings == null)
                return;

            _rings.AddRange(rings);
            foreach (ChainRingHandler ring in _rings)
                ring.Init(this);
        }

        public void OnRingClicked()
        {
            if (LevelSignals.Instance == null)
                return;

            IReadOnlyList<StickHandler> sticks = LevelSignals.Instance.onGetSticks?.Invoke();
            if (sticks == null || sticks.Count == 0)
                return;

            List<ChainRingHandler> bottomLeaves = GetBottomLeafRings();
            if (bottomLeaves.Count == 0)
                return;

            // Vector3 chainPos = transform.position;
            // bottomLeaves.Sort((a, b) =>
            //     (a.transform.position - chainPos).sqrMagnitude.CompareTo(
            //         (b.transform.position - chainPos).sqrMagnitude));

            foreach (ChainRingHandler candidateRing in bottomLeaves)
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

        private static StickHandler FindClosestMatchingStick(ChainRingHandler candidateRing, IReadOnlyList<StickHandler> sticks)
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
        private List<ChainRingHandler> GetBottomLeafRings()
        {
            var leaves = new List<ChainRingHandler>();
            foreach (ChainRingHandler r in _rings)
            {
                if (r.LowerRings != null && r.LowerRings.Count != 0)
                    continue;
                leaves.Add(r);
            }

            return leaves;
        }
    }
}
