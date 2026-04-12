using UnityEngine;
using Assets.Game.Scripts.Signals;
using Assets.Game.Scripts.Enum;
using DG.Tweening;

namespace Assets.Game.Scripts.Level
{
    public class StickHandler : MonoBehaviour
    {
        public const int MaxRings = 3;

        [SerializeField] private float firstRingLocalYOffset;
        [SerializeField] private float ringStackSpacing;

        private int _ringCount;
        private ColorType _colorType;

        public ColorType ColorType => _colorType;

        public void Init(ColorType colorType)
        {
            _colorType = colorType;
            Color? color = ColorSignals.Instance.onGetColor?.Invoke(_colorType);
            if (color != null)
                GetComponentInChildren<MeshRenderer>().material.color = color.Value;
            else Debug.LogError($"Color preset for {_colorType} not found");
        }
        
        public bool CanAcceptRing() => _ringCount < MaxRings;

        public void PlayDisappearAnimation(float duration, System.Action onComplete)
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
            {
                AudioSignals.Instance?.onPlaySound?.Invoke(GameSoundType.StickFull);
                LevelSignals.Instance?.onStickFilled?.Invoke(this);
            }

            return true;
        }
    }
}
