using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.Editor
{
    public partial class LevelGeneratorEditor
    {
        private void DrawStickLayoutPreviewSection()
        {
            EditorGUILayout.LabelField("Stick preview", EditorStyles.boldLabel);

            if (_stickLayoutConfig == null)
            {
                EditorGUILayout.HelpBox(
                    "StickLayoutConfig asset bulunamadı (t:StickLayoutConfig). Grid yerleşimi için gerekli.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox(
                "StickLayoutConfig kurallarına göre grid. Seçili fırça ile tıklayıp sürükleyerek stick boyayın; Save ile JSON’a yazılır.",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            float previewW = Mathf.Min(
                StickPreviewMaxWidth,
                Mathf.Max(160f, EditorGUIUtility.currentViewWidth - 56f));
            Rect previewRect = GUILayoutUtility.GetRect(previewW, StickPreviewHeight, GUILayout.Width(previewW));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            bool isPaintGesture = Event.current.button == 0
                && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag);

            if (isPaintGesture
                && previewRect.Contains(Event.current.mousePosition)
                && _colorPreset != null
                && StickLayoutPreviewGeometry.TryHitStickIndex(
                    previewRect,
                    Event.current.mousePosition,
                    _stickCount,
                    _stickLayoutConfig,
                    StickPaintHitRadius,
                    out int stickIndex))
            {
                _stickColorTypes[stickIndex] = _selectedBrushColorType;
                Event.current.Use();
                Repaint();
            }

            if (Event.current.type == EventType.Repaint)
            {
                Handles.BeginGUI();
                DrawStickPreviewSticks(previewRect);
                Handles.EndGUI();
            }
        }

        private void DrawStickPreviewSticks(Rect previewRect)
        {
            EditorGUI.DrawRect(previewRect, new Color(0.14f, 0.14f, 0.15f, 1f));

            if (_stickLayoutConfig == null || _colorPreset == null)
                return;

            if (!StickLayoutPreviewGeometry.TryGetStickGuiCenters(
                    previewRect,
                    _stickCount,
                    _stickLayoutConfig,
                    out Vector2[] centers))
                return;

            float halfLine = StickPreviewLineLength * 0.5f;
            float ellipseCenterY =
                halfLine + StickPreviewGapLineToEllipse + StickPreviewEllipseRadiusY;

            for (int i = 0; i < _stickCount; i++)
            {
                Vector2 c = centers[i];
                Color baseColor = _colorPreset.GetColor(_stickColorTypes[i]);

                float lineTop = c.y - halfLine;
                float lineBottom = c.y + halfLine;

                Handles.color = baseColor;
                Handles.DrawAAPolyLine(
                    StickPreviewLineWidth,
                    new Vector3(c.x, lineTop, 0f),
                    new Vector3(c.x, lineBottom, 0f));

                var ellipseCenter = new Vector3(c.x, c.y + ellipseCenterY, 0f);
                Matrix4x4 prev = Handles.matrix;
                Handles.matrix = Matrix4x4.TRS(
                    ellipseCenter,
                    Quaternion.identity,
                    new Vector3(StickPreviewEllipseRadiusX, StickPreviewEllipseRadiusY, 1f));
                Handles.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.9f);
                Handles.DrawSolidDisc(Vector3.zero, Vector3.forward, 1f);
                Handles.color = new Color(baseColor.r * 0.38f, baseColor.g * 0.38f, baseColor.b * 0.38f, 0.88f);
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 1f);
                Handles.matrix = prev;
            }
        }
    }
}
