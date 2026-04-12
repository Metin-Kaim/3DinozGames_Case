using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Assets.Game.Scripts.Level;
using Assets.Game.Scripts.Datas.DataValues;

namespace Assets.Game.Scripts.Signals
{
    public class LevelSignals : MonoBehaviour
    {
        public static LevelSignals Instance;

        public Func<StickHandler> onGetStick;
        public Func<IReadOnlyList<StickHandler>> onGetSticks;
        public UnityAction<StickHandler> onStickFilled;
        public Func<int, List<Vector3>> onGetStickLocalPositions;
        public UnityAction<StickHandler> onStickSpawned;
        public Func<bool> onHasCurrentLevel;
        public UnityAction<LevelData> onLevelLoaded;

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
