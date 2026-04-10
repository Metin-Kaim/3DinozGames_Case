using System.Collections.Generic;
using UnityEngine;
using Assets.Game.Scripts.Datas.DataValues;

namespace Assets.Game.Scripts.Datas.UnityValues
{
    [CreateAssetMenu(menuName = "3DinozGames/new Stick Layout Config", fileName = "StickLayoutConfig")]
    public class StickLayoutConfig : ScriptableObject
    {
        public List<StickLayoutRule> rules;

        public Vector2Int GetGrid(int stickCount)
        {
            foreach (var rule in rules)
            {
                if (rule.stickCount == stickCount)
                    return rule.gridSize;
            }

            // fallback algorithm
            int maxColumn = 3;

            int column = Mathf.Min(stickCount, maxColumn);
            int row = Mathf.CeilToInt((float)stickCount / column);

            return new Vector2Int(row, column);
        }
    }
}