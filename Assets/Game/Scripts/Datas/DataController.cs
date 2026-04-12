using System;
using Assets.Game.Scripts.Datas.UnityValues;
using Assets.Game.Scripts.Enum;
using Assets.Game.Scripts.Level;
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

            if (LevelSignals.Instance != null)
                LevelSignals.Instance.onHasCurrentLevel += HasCurrentLevel;
        }

        private void OnDisable()
        {
            if (ColorSignals.Instance != null)
                ColorSignals.Instance.onGetColor -= colorPreset.GetColor;

            if (LevelSignals.Instance != null)
                LevelSignals.Instance.onHasCurrentLevel -= HasCurrentLevel;
        }

        private bool HasCurrentLevel()
        {
            string path = LevelGenerator.FolderName;

            int levelIndex = LevelSignals.Instance.onGetCurrentLevelIndex?.Invoke() ?? 1;

            string levelDataPath = $"{path}/Level_{levelIndex}";
            TextAsset textAsset = Resources.Load<TextAsset>(levelDataPath);
            return textAsset != null;
        }
    }
}
