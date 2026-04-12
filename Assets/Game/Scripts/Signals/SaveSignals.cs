using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Game.Scripts.Signals
{
    public class SaveSignals : MonoBehaviour
    {
        public static SaveSignals Instance;

        public UnityAction<int> onSaveLevelIndex;
        public Func<int> onGetSavedLevelIndex = () => 1;

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