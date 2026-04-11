using System;
using Assets.Game.Scripts.Enum;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.Editor
{
    public partial class LevelGeneratorEditor
    {
        private void DrawHookChainSegmentGui(int hookIndex)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));

            EditorGUILayout.LabelField($"Hook {hookIndex + 1}", EditorStyles.boldLabel);

            _baseRingCountPerHook[hookIndex] = EditorGUILayout.IntSlider(
                new GUIContent("Base ring count"),
                _baseRingCountPerHook[hookIndex],
                MinBaseRing,
                MaxBaseRing);

            _forkPerHook[hookIndex] = EditorGUILayout.Toggle(new GUIContent("Fork"), _forkPerHook[hookIndex]);

            if (_forkPerHook[hookIndex])
            {
                EditorGUI.indentLevel++;
                _forkBranch1BaseRingCountPerHook[hookIndex] = EditorGUILayout.IntSlider(
                    new GUIContent("Branch 1 base count"),
                    _forkBranch1BaseRingCountPerHook[hookIndex],
                    MinBaseRing,
                    MaxBaseRing);
                _forkBranch2BaseRingCountPerHook[hookIndex] = EditorGUILayout.IntSlider(
                    new GUIContent("Branch 2 base count"),
                    _forkBranch2BaseRingCountPerHook[hookIndex],
                    MinBaseRing,
                    MaxBaseRing);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(12f);

            float ringsHeight = GetSegmentRingsAreaHeight(
                _baseRingCountPerHook[hookIndex],
                _forkPerHook[hookIndex],
                _forkBranch1BaseRingCountPerHook[hookIndex],
                _forkBranch2BaseRingCountPerHook[hookIndex]);

            Rect ringsRect = GUILayoutUtility.GetRect(10f, ringsHeight, GUILayout.ExpandWidth(true));

            bool isPaintGesture = Event.current.button == 0
                && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag);

            if (isPaintGesture
                && ringsRect.Contains(Event.current.mousePosition)
                && _colorPreset != null
                && TryHitTestRing(hookIndex, ringsRect, Event.current.mousePosition, out int segment, out int ringIndex))
            {
                _ringColorTypePerHook[hookIndex, segment, ringIndex] = _selectedBrushColorType;
                Event.current.Use();
                Repaint();
            }

            if (Event.current.type == EventType.Repaint)
            {
                Handles.BeginGUI();
                DrawHookSegmentRings(hookIndex, ringsRect);
                Handles.EndGUI();
            }

            EditorGUILayout.EndVertical();
        }

        private bool TryHitTestRing(
            int hookIndex,
            Rect ringsRect,
            Vector2 mouse,
            out int segment,
            out int ringIndex)
        {
            segment = 0;
            ringIndex = 0;

            float cx = ringsRect.x + ringsRect.width * 0.5f;
            float yContent = ringsRect.y + HookGripHeight + RingsTopPadding;
            int trunk = _baseRingCountPerHook[hookIndex];
            bool fork = _forkPerHook[hookIndex];
            float hitR = FixedRingRadius + 3f;

            for (int i = 0; i < trunk; i++)
            {
                float cy = yContent + FixedRingRadius + i * FixedRingCenterStep;
                if (Vector2.Distance(mouse, new Vector2(cx, cy)) <= hitR)
                {
                    segment = RingSegmentTrunk;
                    ringIndex = i;
                    return true;
                }
            }

            if (!fork)
                return false;

            float trunkStackH = StackHeight(trunk);
            float yBranch = yContent + trunkStackH + ForkGapAfterTrunk;
            float midX = ringsRect.x + ringsRect.width * 0.5f;
            float branchHalfSpacing = Mathf.Clamp(ringsRect.width * 0.045f, FixedRingRadius + 0.5f, 14f);
            float leftX = midX - branchHalfSpacing;
            float rightX = midX + branchHalfSpacing;

            int b1 = _forkBranch1BaseRingCountPerHook[hookIndex];
            for (int i = 0; i < b1; i++)
            {
                float cy = yBranch + FixedRingRadius + i * FixedRingCenterStep;
                if (Vector2.Distance(mouse, new Vector2(leftX, cy)) <= hitR)
                {
                    segment = RingSegmentBranch1;
                    ringIndex = i;
                    return true;
                }
            }

            int b2 = _forkBranch2BaseRingCountPerHook[hookIndex];
            for (int i = 0; i < b2; i++)
            {
                float cy = yBranch + FixedRingRadius + i * FixedRingCenterStep;
                if (Vector2.Distance(mouse, new Vector2(rightX, cy)) <= hitR)
                {
                    segment = RingSegmentBranch2;
                    ringIndex = i;
                    return true;
                }
            }

            return false;
        }

        private float GetSegmentRingsAreaHeight(int trunkRings, bool fork, int branch1Rings, int branch2Rings)
        {
            float body = HookGripHeight + RingsTopPadding + StackHeight(trunkRings) + RingsBottomPadding;

            if (!fork)
                return body;

            int maxBranch = Mathf.Max(branch1Rings, branch2Rings);
            return body - RingsBottomPadding + ForkGapAfterTrunk + StackHeight(maxBranch) + RingsBottomPadding;
        }

        private void DrawHookSegmentRings(int hookIndex, Rect area)
        {
            float cx = area.x + area.width * 0.5f;
            EditorGUI.DrawRect(
                new Rect(cx - 2.5f, area.y, 5f, HookGripHeight),
                new Color(0.52f, 0.54f, 0.58f, 1f));

            float yContent = area.y + HookGripHeight + RingsTopPadding;

            Color Resolve(ColorType ct)
            {
                if (_colorPreset != null)
                    return _colorPreset.GetColor(ct);
                return Color.white;
            }

            int trunk = _baseRingCountPerHook[hookIndex];

            if (!_forkPerHook[hookIndex])
            {
                DrawFixedVerticalRingStack(yContent, cx, trunk, hookIndex, RingSegmentTrunk, Resolve);
                return;
            }

            float trunkStackH = StackHeight(trunk);
            DrawFixedVerticalRingStack(yContent, cx, trunk, hookIndex, RingSegmentTrunk, Resolve);

            float yBranch = yContent + trunkStackH + ForkGapAfterTrunk;
            float midX = area.x + area.width * 0.5f;
            float branchHalfSpacing = Mathf.Clamp(area.width * 0.045f, FixedRingRadius + 0.5f, 14f);
            float leftX = midX - branchHalfSpacing;
            float rightX = midX + branchHalfSpacing;

            int b1 = _forkBranch1BaseRingCountPerHook[hookIndex];
            int b2 = _forkBranch2BaseRingCountPerHook[hookIndex];

            DrawFixedVerticalRingStack(yBranch, leftX, b1, hookIndex, RingSegmentBranch1, Resolve);
            DrawFixedVerticalRingStack(yBranch, rightX, b2, hookIndex, RingSegmentBranch2, Resolve);
        }

        private float StackHeight(int count)
        {
            if (count <= 0)
                return 0f;

            return 2f * FixedRingRadius + Mathf.Max(0, count - 1) * FixedRingCenterStep;
        }

        private void DrawFixedVerticalRingStack(
            float topY,
            float centerX,
            int ringCount,
            int hookIndex,
            int segment,
            Func<ColorType, Color> resolveColor)
        {
            if (ringCount <= 0)
                return;

            for (int i = 0; i < ringCount; i++)
            {
                ColorType ct = _ringColorTypePerHook[hookIndex, segment, i];
                Handles.color = resolveColor(ct);
                float cy = topY + FixedRingRadius + i * FixedRingCenterStep;
                Handles.DrawSolidDisc(new Vector3(centerX, cy, 0f), Vector3.forward, FixedRingRadius);
            }
        }
    }
}
