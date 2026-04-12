using TMPro;
using UnityEngine;
using Assets.Game.Scripts.Signals;
using Assets.Game.Scripts.Datas.DataValues;

namespace Assets.Game.Scripts.UI
{
    public class LevelCounterPanelHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelCounterText;

        private void OnEnable()
        {
            if (LevelSignals.Instance == null)
                return;

            LevelSignals.Instance.onLevelLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            if (LevelSignals.Instance == null)
                return;

            LevelSignals.Instance.onLevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded(LevelData levelData, int levelIndex)
        {
            levelCounterText.text = "Level " + levelIndex;
        }
    }
}