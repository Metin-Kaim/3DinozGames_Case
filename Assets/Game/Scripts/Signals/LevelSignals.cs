using System;
using UnityEngine;
using UnityEngine.Events;
using Assets.Game.Scripts.Level;

namespace Assets.Game.Scripts.Signals
{
    public class LevelSignals : MonoBehaviour
    {
        public static LevelSignals Instance;

        public Func<StickHandler> onGetStick;
        public UnityAction<StickHandler> onStickFilled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
