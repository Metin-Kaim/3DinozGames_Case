using System.Collections;
using System.Collections.Generic;
using Assets.Game.Scripts.Signals;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameEndPanelController : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject failPanel;

    [SerializeField] private GameObject panelContainer;


    private void OnEnable()
    {
        GameSignals.Instance.onGameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        if (GameSignals.Instance == null)
            return;

        GameSignals.Instance.onGameEnded -= OnGameEnded;
    }

    private void OnGameEnded(bool isWin, float delay)
    {
        if (delay > 0f)
            DOVirtual.DelayedCall(delay, () =>
            {
                ShowPanel(isWin);
            }).SetTarget(this);
        else
            ShowPanel(isWin);
    }

    private void ShowPanel(bool isWin)
    {
        GameObject panel = isWin ? winPanel : failPanel;
        GameObject gameEndPanel = Instantiate(panel, panelContainer.transform);

        Button button = gameEndPanel.GetComponentInChildren<Button>(true);
        button.onClick.AddListener(() =>
        {
            GameSignals.Instance.onLoadScene?.Invoke();
        });

        gameEndPanel.SetActive(true);
    }
}
