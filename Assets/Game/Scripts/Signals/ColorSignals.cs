using System;
using Assets.Game.Scripts.Enum;
using UnityEngine;

namespace Assets.Game.Scripts.Signals
{
    public class ColorSignals : MonoBehaviour
    {
        public static ColorSignals Instance;

        public Func<ColorType, Color> onGetColor;

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
