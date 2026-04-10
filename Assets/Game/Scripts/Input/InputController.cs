using UnityEngine;
using Assets.Game.Scripts.Level;

namespace Assets.Game.Scripts.Input
{
    public class InputController : MonoBehaviour
    {
        [SerializeField] private LayerMask clickableRingLayer;
        [SerializeField] private float maxRayDistance = 100f;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (_mainCamera == null)
                    return;

                Ray ray = _mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);

                if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, clickableRingLayer))
                    return;

                ChainRingHandler ring = hit.collider.GetComponentInParent<ChainRingHandler>();
                ring?.NotifyClicked();
            }

        }
    }
}
