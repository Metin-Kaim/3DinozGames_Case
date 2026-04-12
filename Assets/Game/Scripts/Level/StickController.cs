using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Assets.Game.Scripts.Signals;

namespace Assets.Game.Scripts.Level
{
    public class StickController : MonoBehaviour
    {
        [Header("Sticks — Runtime")]
        [SerializeField] private float stickDisappearDuration = 0.4f;
        [SerializeField] private float stickRealignDuration = 0.35f;
        [Tooltip("Dolu stick listeden düştükten sonra kalanların hizalanması için kısa bekleme (saniye). 0 = anında.")]
        [SerializeField] private float stickRealignDelay = 0.08f;

        private readonly List<StickHandler> _sticks = new();

        private void OnEnable()
        {
            if (LevelSignals.Instance == null)
                return;

            LevelSignals.Instance.onStickSpawned += OnStickSpawned;
            LevelSignals.Instance.onStickFilled += OnStickFilled;
            LevelSignals.Instance.onGetStick += GetNextAvailableStick;
            LevelSignals.Instance.onGetSticks += () => _sticks;
        }

        private void OnDisable()
        {
            DOTween.Kill(this);

            if (LevelSignals.Instance == null)
                return;

            LevelSignals.Instance.onStickSpawned -= OnStickSpawned;
            LevelSignals.Instance.onStickFilled -= OnStickFilled;
            LevelSignals.Instance.onGetStick -= GetNextAvailableStick;
            LevelSignals.Instance.onGetSticks -= () => _sticks;
        }

        private void OnStickSpawned(StickHandler stick)
        {
            if (stick != null)
                _sticks.Add(stick);
        }

        private void OnStickFilled(StickHandler stick)
        {
            if (stick == null)
                return;

            if (!_sticks.Remove(stick))
                return;

            if (_sticks.Count == 0)
            {
                GameSignals.Instance?.onGameEnded?.Invoke(true, stickDisappearDuration);
            }

            ScheduleRealignSticks();

            stick.PlayDisappearAnimation(stickDisappearDuration, () =>
            {
                if (stick != null)
                    Destroy(stick.gameObject);
            });
        }

        private void ScheduleRealignSticks()
        {
            if (stickRealignDelay <= 0f)
            {
                RealignSticks();
                return;
            }

            DOVirtual.DelayedCall(stickRealignDelay, RealignSticksIfAlive).SetTarget(this);
        }

        private void RealignSticksIfAlive()
        {
            if (this == null)
                return;

            RealignSticks();
        }

        private void RealignSticks()
        {
            int stickCount = _sticks.Count;
            if (stickCount == 0)
                return;

            List<Vector3> positions = LevelSignals.Instance?.onGetStickLocalPositions?.Invoke(stickCount);
            if (positions == null || positions.Count < stickCount)
                return;

            for (int i = 0; i < stickCount; i++)
            {
                StickHandler stick = _sticks[i];
                if (stick == null)
                    continue;

                stick.transform.DOKill();
                stick.transform.DOLocalMove(positions[i], stickRealignDuration).SetEase(Ease.OutQuad);
            }
        }

        private StickHandler GetNextAvailableStick()
        {
            foreach (StickHandler stick in _sticks)
            {
                if (stick != null && stick.CanAcceptRing())
                    return stick;
            }

            return null;
        }
    }
}
