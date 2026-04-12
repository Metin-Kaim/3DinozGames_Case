using Assets.Game.Scripts.Datas.DataValues;
using Assets.Game.Scripts.Signals;
using TMPro;
using UnityEngine;

namespace Assets.Game.Scripts.UI
{
    public class TimerPanelController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        private float _timeLimitSeconds;
        private bool _isActive;

        private void OnEnable()
        {
            ResetTimer();
            _isActive = true;
            GameSignals.Instance.onGameEnded += OnGameEnded;
            if (LevelSignals.Instance != null)
                LevelSignals.Instance.onLevelLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            if (GameSignals.Instance != null)
                GameSignals.Instance.onGameEnded -= OnGameEnded;
            if (LevelSignals.Instance != null)
                LevelSignals.Instance.onLevelLoaded -= OnLevelLoaded;
        }

        private void Update()
        {
            if (!_isActive)
                return;

            _timeLimitSeconds -= Time.deltaTime;
            if (_timeLimitSeconds <= 0f)
            {
                _timeLimitSeconds = 0f;
                UpdateTimerText();
                OnTimeExpired();
                return;
            }

            UpdateTimerText();
        }

        public void ResetTimer()
        {
            _timeLimitSeconds = 0;
            UpdateTimerText();
        }

        private void UpdateTimerText()
        {
            if (timerText == null)
                return;

            timerText.text = _timeLimitSeconds.ToString("F0");
        }

        private void OnTimeExpired()
        {
            if (!_isActive)
                return;
            _isActive = false;
            GameSignals.Instance?.onGameEnded?.Invoke(false, 0f);
        }

        private void OnGameEnded(bool isWin, float delay)
        {
            _isActive = false;
        }

        private void OnLevelLoaded(LevelData levelData, int levelIndex)
        {
            _timeLimitSeconds = levelData.timeLimitSeconds;
            UpdateTimerText();
        }
    }
}
