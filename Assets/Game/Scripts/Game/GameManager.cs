using System.Collections;
using System.Collections.Generic;
using Assets.Game.Scripts.Signals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Game.Scripts.Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName;
        [SerializeField] private string _comingSoonSceneName;


        private void OnEnable()
        {
            GameSignals.Instance.onGameEnded += OnGameEnded;
            GameSignals.Instance.onLoadScene += LoadScene;
        }
        private void OnDisable()
        {
            if (GameSignals.Instance == null)
                return;

            GameSignals.Instance.onGameEnded -= OnGameEnded;
            GameSignals.Instance.onLoadScene -= LoadScene;
        }

        private void OnGameEnded(bool isWin, float delay)
        {
            InputSignals.Instance.OnSetClickable?.Invoke(false);
        }

        private void LoadScene()
        {
            if (LevelSignals.Instance.onHasCurrentLevel.Invoke())
            {
                ReloadScene();
            }
            else
            {
                LoadComingSoonScene();
            }
        }

        private void ReloadScene()
        {
            StartCoroutine(ReloadSceneCoroutine());
        }

        private IEnumerator ReloadSceneCoroutine()
        {
            yield return SceneManager.LoadSceneAsync(_gameSceneName);
        }

        private void LoadComingSoonScene()
        {
            StartCoroutine(LoadComingSoonSceneCoroutine());
        }

        private IEnumerator LoadComingSoonSceneCoroutine()
        {
            yield return SceneManager.LoadSceneAsync(_comingSoonSceneName);
        }
    }
}