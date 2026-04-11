using Assets.Game.Scripts.Datas.UnityValues;
using Assets.Game.Scripts.Enum;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.Editor
{
    public partial class LevelGeneratorEditor : EditorWindow
    {
        internal const int MaxStickCount = 5;
        internal const int MaxHookCount = 3;
        internal const int MinBaseRing = 1;
        internal const int MaxBaseRing = 10;

        /// <summary>Oyun kuralları: her stick en fazla bu kadar halka kabul eder.</summary>
        internal const int RingsPerStick = 3;

        internal const int RingSegmentTrunk = 0;
        internal const int RingSegmentBranch1 = 1;
        internal const int RingSegmentBranch2 = 2;

        internal const float FixedRingRadius = 10f;
        internal static readonly float FixedRingCenterStep = FixedRingRadius * 2f + 3f;
        internal const float HookGripHeight = 7f;
        internal const float RingsTopPadding = 10f;
        internal const float RingsBottomPadding = 4f;
        internal const float ForkGapAfterTrunk = 2f;

        internal const float SwatchSize = 32f;
        internal const float SwatchRowHeight = 46f;

        internal const float StickPreviewHeight = 132f;
        internal const float StickPreviewMaxWidth = 300f;
        internal const float StickPaintHitRadius = 20f;

        internal const float StickPreviewLineLength = 18f;
        internal const float StickPreviewLineWidth = 4f;
        internal const float StickPreviewGapLineToEllipse = 3f;
        internal const float StickPreviewEllipseRadiusX = 10f;
        internal const float StickPreviewEllipseRadiusY = 4f;

        internal const float JsonPanelViewportHeight = 200f;

        private int _stickCount = 1;
        private int _hookCount = 1;

        private readonly int[] _baseRingCountPerHook = { 1, 1, 1 };
        private readonly bool[] _forkPerHook = new bool[MaxHookCount];
        private readonly int[] _forkBranch1BaseRingCountPerHook = { 1, 1, 1 };
        private readonly int[] _forkBranch2BaseRingCountPerHook = { 1, 1, 1 };

        private readonly ColorType[,,] _ringColorTypePerHook = new ColorType[MaxHookCount, 3, MaxBaseRing];
        private readonly ColorType[] _stickColorTypes = new ColorType[MaxStickCount];

        private ColorType _selectedBrushColorType = ColorType.Red;
        private ColorPreset _colorPreset;
        private StickLayoutConfig _stickLayoutConfig;

        private Vector2 _scrollPosition;
        private Vector2 _jsonScrollPosition;

        private int _levelFileIndex = 1;
        private int _newLevelSlotNumber = 1;

        [MenuItem("Tools/3Dinoz/LevelGeneratorEditor")]
        public static void Open()
        {
            var window = GetWindow<LevelGeneratorEditor>();
            window.titleContent = new GUIContent("LevelGeneratorEditor");
            window.Show();
        }

        private void OnEnable()
        {
            LoadEditorAssets();
        }

        private void LoadEditorAssets()
        {
            string[] colorGuids = AssetDatabase.FindAssets("t:ColorPreset");
            if (colorGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(colorGuids[0]);
                _colorPreset = AssetDatabase.LoadAssetAtPath<ColorPreset>(path);
                _colorPreset?.Init();
            }

            string[] layoutGuids = AssetDatabase.FindAssets("t:StickLayoutConfig");
            if (layoutGuids.Length > 0)
            {
                _stickLayoutConfig = AssetDatabase.LoadAssetAtPath<StickLayoutConfig>(
                    AssetDatabase.GUIDToAssetPath(layoutGuids[0]));
            }
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Space(8f);
            DrawLevelDataSaveLoadSection();

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Level Generator", EditorStyles.boldLabel);

            _stickCount = EditorGUILayout.IntSlider(new GUIContent("Stick count"), _stickCount, 1, MaxStickCount);
            _hookCount = EditorGUILayout.IntSlider(new GUIContent("Hook count"), _hookCount, 1, MaxHookCount);

            EditorGUILayout.Space(6f);
            DrawBrushPalette();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Hook chains", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            for (int hookIndex = 0; hookIndex < _hookCount; hookIndex++)
                DrawHookChainSegmentGui(hookIndex);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(12f);
            DrawStickLayoutPreviewSection();

            EditorGUILayout.Space(12f);
            DrawLevelValidationSection();

            EditorGUILayout.Space(12f);
            DrawJsonExportSection();

            EditorGUILayout.EndScrollView();
        }
    }
}
