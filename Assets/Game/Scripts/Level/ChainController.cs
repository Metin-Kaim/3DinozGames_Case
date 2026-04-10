using System.Collections.Generic;
using UnityEngine;
using Assets.Game.Scripts.Signals;

namespace Assets.Game.Scripts.Level
{
    public class ChainController : MonoBehaviour
    {
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

            StickHandler stick = LevelSignals.Instance.onGetStick?.Invoke();
            if (stick == null || !stick.CanAcceptRing())
                return;

            ChainRingHandler bottom = GetBottomLeafRing();
            if (bottom == null)
                return;

            _rings.Remove(bottom);
            bottom.DetachFromChainForStick();
            bottom.AttachToStickWithTween(stick, attachTweenDuration);
        }

        private ChainRingHandler GetBottomLeafRing()
        {
            ChainRingHandler pick = null;
            float bestY = float.PositiveInfinity;

            foreach (ChainRingHandler r in _rings)
            {
                if (r.LowerRings == null || r.LowerRings.Count != 0)
                    continue;

                float y = r.transform.localPosition.y;
                if (y < bestY)
                {
                    bestY = y;
                    pick = r;
                }
            }

            return pick;
        }
    }
}