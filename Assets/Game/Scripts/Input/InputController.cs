using UnityEngine;
using Assets.Game.Scripts.Level;
using Assets.Game.Scripts.Signals;

namespace Assets.Game.Scripts.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private LayerMask clickableRingLayer;
        [SerializeField] private float maxRayDistance = 30f;

        private Camera _mainCamera;
        private bool _isClickable = true;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            InputSignals.Instance.OnSetClickable += SetClickable;
        }
        private void OnDisable()
        {
            if (InputSignals.Instance == null)
                return;

            InputSignals.Instance.OnSetClickable -= SetClickable;
        }

        private void Update()
        {
            if (!_isClickable)
                return;

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (_mainCamera == null)
                    return;

                Ray ray = _mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);

                if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, clickableRingLayer))
                    return;

                RingHandler ring = hit.collider.GetComponentInParent<RingHandler>();
                ring?.NotifyClicked();
            }

        }

        public void SetClickable(bool isClickable)
        {
            _isClickable = isClickable;
        }
    }
}
