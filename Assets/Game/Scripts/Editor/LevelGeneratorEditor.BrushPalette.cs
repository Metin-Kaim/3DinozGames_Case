using Assets.Game.Scripts.Enum;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.Editor
{
    public partial class LevelGeneratorEditor
    {
        private void DrawBrushPalette()
        {
            EditorGUILayout.LabelField("Brush color", EditorStyles.miniBoldLabel);

            if (_colorPreset == null)
            {
                EditorGUILayout.HelpBox("ColorPreset asset not found.", MessageType.Warning);
                return;
            }

            var entries = _colorPreset.PresetEntries;
            if (entries == null || entries.Count == 0)
            {
                EditorGUILayout.HelpBox("ColorPreset has no entries.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(4f);

            var plusStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };

            foreach (var entry in entries)
            {
                Color swatchColor = entry.color;
                ColorType ct = entry.colorType;
                bool selected = _selectedBrushColorType == ct;

                EditorGUILayout.BeginVertical(GUILayout.Height(SwatchRowHeight), GUILayout.Width(SwatchSize + 4f));

                Rect columnRect = GUILayoutUtility.GetRect(SwatchSize + 4f, SwatchRowHeight);

                if (Event.current.type == EventType.Repaint)
                {
                    var swatchRect = new Rect(
                        columnRect.x + (columnRect.width - SwatchSize) * 0.5f,
                        columnRect.y + (selected ? 14f : 2f),
                        SwatchSize,
                        SwatchSize);

                    if (selected)
                        GUI.Label(new Rect(columnRect.x, columnRect.y, columnRect.width, 14f), "+", plusStyle);

                    EditorGUI.DrawRect(
                        new Rect(swatchRect.x - 1f, swatchRect.y - 1f, swatchRect.width + 2f, swatchRect.height + 2f),
                        selected ? new Color(1f, 1f, 1f, 0.95f) : new Color(0f, 0f, 0f, 0.35f));
                    EditorGUI.DrawRect(swatchRect, swatchColor);
                }

                if (Event.current.type == EventType.MouseDown
                    && columnRect.Contains(Event.current.mousePosition))
                {
                    _selectedBrushColorType = ct;
                    Event.current.Use();
                    Repaint();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
