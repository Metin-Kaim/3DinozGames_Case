using Assets.Game.Scripts.Signals;
using TMPro;
using UnityEngine;

namespace Assets.Game.Scripts.UI
{
    public class TimerPanelController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [Tooltip("Geri sayım süresi (saniye).")]
        [SerializeField] private int timeLimitSeconds = 60;

        private float _remainingSeconds;
        private bool _isActive;

        private void OnEnable()
        {
            ResetTimer();
            _isActive = true;
            GameSignals.Instance.onGameEnded += OnGameEnded;
        }

        private void OnDisable()
        {
            if (GameSignals.Instance == null)
                return;

            GameSignals.Instance.onGameEnded -= OnGameEnded;
        }

        private void Update()
        {
            if (!_isActive)
                return;

            _remainingSeconds -= Time.deltaTime;
            if (_remainingSeconds <= 0f)
            {
                _remainingSeconds = 0f;
                UpdateTimerText();
                OnTimeExpired();
                return;
            }

            UpdateTimerText();
        }

        public void ResetTimer()
        {
            _remainingSeconds = timeLimitSeconds;
            UpdateTimerText();
        }

        private void UpdateTimerText()
        {
            if (timerText == null)
                return;

            timerText.text = _remainingSeconds.ToString("F0");
        }

        private void OnTimeExpired()
        {
            GameSignals.Instance?.onGameEnded?.Invoke(false, 0f);
        }

        private void OnGameEnded(bool isWin, float delay)
        {
            _isActive = false;
        }
    }
}
