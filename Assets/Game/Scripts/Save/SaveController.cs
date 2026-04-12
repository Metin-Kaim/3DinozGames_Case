using Assets.Game.Scripts.Signals;
using UnityEngine;

namespace Assets.Game.Scripts.Save
{
    public class SaveController : MonoBehaviour
    {
        private const string LevelIndexKey = "LevelIndex";

        private void OnEnable()
        {
            if (SaveSignals.Instance == null)
                return;

            SaveSignals.Instance.onSaveLevelIndex += SaveLevelIndex;
            SaveSignals.Instance.onGetSavedLevelIndex += GetLevelIndex;
        }
        private void OnDisable()
        {
            if (SaveSignals.Instance == null)
                return;

            SaveSignals.Instance.onSaveLevelIndex -= SaveLevelIndex;
            SaveSignals.Instance.onGetSavedLevelIndex -= GetLevelIndex;
        }

        private void SaveLevelIndex(int levelIndex)
        {
            PlayerPrefs.SetInt(LevelIndexKey, levelIndex);
        }

        private int GetLevelIndex()
        {
            return PlayerPrefs.GetInt(LevelIndexKey, 1);
        }
    }
}