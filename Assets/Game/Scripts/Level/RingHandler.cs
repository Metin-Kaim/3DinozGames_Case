using System.Collections.Generic;
using Assets.Game.Scripts.Enum;
using Assets.Game.Scripts.Signals;
using DG.Tweening;
using UnityEngine;

namespace Assets.Game.Scripts.Level
{
    public class RingHandler : MonoBehaviour
    {
        [SerializeField] private RingHandler upperRing;
        [SerializeField] private List<RingHandler> lowerRings = new();

        private ChainController _chainHook;
        private ColorType _colorType;

        public ColorType ColorType => _colorType;
        public List<RingHandler> LowerRings => lowerRings;

        public void Init(ChainController chainHook, ColorType colorType)
        {
            _chainHook = chainHook;
            _colorType = colorType;
            Color? color = ColorSignals.Instance.onGetColor?.Invoke(_colorType);
            if (color != null)
                GetComponentInChildren<MeshRenderer>().material.color = color.Value;
            else Debug.LogError($"Color preset for {_colorType} not found");
        }

        public void NotifyClicked()
        {
            _chainHook?.OnRingClicked();
        }

        public void ConnectAbove(RingHandler upper)
        {
            upperRing = upper;
            upper?.lowerRings.Add(this);
        }

        public void DetachFromChainForStick()
        {
            if (upperRing != null)
            {
                upperRing.lowerRings.Remove(this);
                upperRing = null;
            }
        }

        public void AttachToStickWithTween(StickHandler stick, float duration)
        {
            if (stick == null)
                return;

            if (!stick.TryReserveNextRingSlot(out Vector3 localTarget))
                return;

            transform.SetParent(stick.transform);
            transform.DOKill();
            transform.DOLocalMove(localTarget, duration).SetEase(Ease.OutBack, 0.75f);
            transform.DOLocalRotate(Vector3.zero, duration / 2f).SetEase(Ease.Linear);
        }
    }
}
