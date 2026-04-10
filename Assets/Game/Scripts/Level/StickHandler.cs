using UnityEngine;
using Assets.Game.Scripts.Signals;
using System;
using DG.Tweening;

namespace Assets.Game.Scripts.Level
{
    public class StickHandler : MonoBehaviour
    {
        public const int MaxRings = 3;

        [SerializeField] private float firstRingLocalYOffset;
        [SerializeField] private float ringStackSpacing;

        private int _ringCount;

        public bool CanAcceptRing() => _ringCount < MaxRings;

        public void PlayDisappearAnimation(float duration, Action onComplete)
        {
            transform.DOKill();
            transform.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        public bool TryReserveNextRingSlot(out Vector3 localTargetPosition)
        {
            localTargetPosition = default;

            if (!CanAcceptRing())
                return false;

            localTargetPosition = new Vector3(0f, firstRingLocalYOffset + _ringCount * ringStackSpacing, 0f);
            _ringCount++;

            if (_ringCount >= MaxRings)
                LevelSignals.Instance?.onStickFilled?.Invoke(this);

            return true;
        }
    }
}
