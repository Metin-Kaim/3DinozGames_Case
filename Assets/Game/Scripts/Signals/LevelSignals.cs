using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Assets.Game.Scripts.Level;

namespace Assets.Game.Scripts.Signals
{
    public class LevelSignals : MonoBehaviour
    {
        public static LevelSignals Instance;

        public Func<StickHandler> onGetStick;
        public Func<IReadOnlyList<StickHandler>> onGetSticks;
        public UnityAction<StickHandler> onStickFilled;

        /// <summary>Stick sayısına göre yerel grid pozisyonları (LevelGenerator kaynaklı).</summary>
        public Func<int, List<Vector3>> onGetStickLocalPositions;

        /// <summary>Her stick spawn edildiğinde tetiklenir; StickController listeye ekler.</summary>
        public UnityAction<StickHandler> onStickSpawned;

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
