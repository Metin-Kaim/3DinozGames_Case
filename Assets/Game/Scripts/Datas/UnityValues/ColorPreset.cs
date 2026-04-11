using System.Collections.Generic;
using Assets.Game.Scripts.Datas.DataValues;
using Assets.Game.Scripts.Enum;
using UnityEngine;

namespace Assets.Game.Scripts.Datas.UnityValues
{
    [CreateAssetMenu(menuName = "3DinozGames/new Color Preset", fileName = "ColorPreset")]
    public class ColorPreset : ScriptableObject
    {
        [SerializeField] private List<ColorPresetEntry> colorPresetEntries;

        private Dictionary<ColorType, Color> colorPresetMap;

        public IReadOnlyList<ColorPresetEntry> PresetEntries => colorPresetEntries;

        public void Init()
        {
            if(colorPresetMap != null) return;
            
            colorPresetMap = new Dictionary<ColorType, Color>();
            foreach (var entry in colorPresetEntries)
            {
                colorPresetMap[entry.colorType] = entry.color;
            }
        }

        public Color GetColor(ColorType colorType)
        {
            if (!colorPresetMap.TryGetValue(colorType, out var color))
            {
                Debug.LogError($"Color preset for {colorType} not found");
                return Color.white;
            }
            return color;
        }
    }
}
