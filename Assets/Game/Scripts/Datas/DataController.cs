using System;
using Assets.Game.Scripts.Datas.UnityValues;
using Assets.Game.Scripts.Enum;
using Assets.Game.Scripts.Signals;
using UnityEngine;

namespace Assets.Game.Scripts.Datas
{
    public class DataController : MonoBehaviour
    {
        [SerializeField] private ColorPreset colorPreset;

        private void Awake()
        {
            if (colorPreset == null)
                return;

            colorPreset.Init();
        }

        private void OnEnable()
        {
            if (ColorSignals.Instance != null)
                ColorSignals.Instance.onGetColor += colorPreset.GetColor;
        }

        private void OnDisable()
        {
            if (ColorSignals.Instance != null)
                ColorSignals.Instance.onGetColor -= colorPreset.GetColor;
        }

    }
}
