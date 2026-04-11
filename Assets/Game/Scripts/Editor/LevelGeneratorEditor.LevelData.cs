using System;
using System.Collections.Generic;
using System.IO;
using Assets.Game.Scripts.Datas.DataValues;
using Assets.Game.Scripts.Enum;
using Assets.Game.Scripts.Level;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.Editor
{
    public partial class LevelGeneratorEditor
    {
        private void DrawLevelDataSaveLoadSection()
        {
            EditorGUILayout.LabelField("Level data (Resources/LevelDatas)", EditorStyles.boldLabel);

            int[] scanned = LevelDataFileUtility.ScanExistingLevelIndices();
            int[] popupValues = MergeLevelIndicesForPopup(scanned, _levelFileIndex);
            var popupLabels = new string[popupValues.Length];
            for (int i = 0; i < popupValues.Length; i++)
                popupLabels[i] = $"Level_{popupValues[i]}";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Level slot",
                    "Kayıt: Level_{n}.json — diskteki dosyalar + mevcut numara listelenir."),
                GUILayout.Width(78f));
            _levelFileIndex = EditorGUILayout.IntPopup(_levelFileIndex, popupLabels, popupValues);
            _levelFileIndex = Mathf.Max(1, _levelFileIndex);

            if (GUILayout.Button("Refresh list", GUILayout.Width(100f)))
                Repaint();

            if (GUILayout.Button("Save", GUILayout.Width(80f)))
                SaveLevelJsonToResources(_levelFileIndex);

            if (GUILayout.Button("Load", GUILayout.Width(80f)))
                LoadLevelJsonFromResources(_levelFileIndex);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                new GUIContent(
                    "New slot #",
                    "Yeni slot numarası. Add new & Save: bu numaraya geçer ve mevcut editör verisini Level_{n}.json olarak kaydeder."),
                GUILayout.Width(78f));
            _newLevelSlotNumber = EditorGUILayout.IntField(_newLevelSlotNumber);
            _newLevelSlotNumber = Mathf.Max(1, _newLevelSlotNumber);

            if (GUILayout.Button("Add new & Save", GUILayout.Width(120f)))
            {
                _levelFileIndex = _newLevelSlotNumber;
                SaveLevelJsonToResources(_levelFileIndex);
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            if (scanned.Length == 0)
                EditorGUILayout.HelpBox("Henüz Level_*.json yok; Save ile oluşturabilirsin.", MessageType.Info);
            else
                EditorGUILayout.HelpBox(
                    $"Bulunan dosyalar: {scanned.Length} adet. Seçili: Level_{_levelFileIndex}.json",
                    MessageType.None);
        }

        private int[] MergeLevelIndicesForPopup(int[] scanned, int currentLevelIndex)
        {
            var set = new SortedSet<int>();
            foreach (int n in scanned)
            {
                if (n >= 1)
                    set.Add(n);
            }

            set.Add(Mathf.Max(1, currentLevelIndex));

            var arr = new int[set.Count];
            set.CopyTo(arr);
            return arr;
        }

        private void SaveLevelJsonToResources(int levelIndex1Based)
        {
            try
            {
                LevelDataFileUtility.EnsureDirectoryExists();
                string path = LevelDataFileUtility.GetAbsoluteJsonPath(levelIndex1Based);
                string json = BuildLevelGeneratorJson();
                File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                ShowNotification(new GUIContent($"Kaydedildi — Level_{levelIndex1Based}.json"));
            }
            catch (Exception e)
            {
                ShowNotification(new GUIContent($"Kayıt hatası\n{e.Message}"));
            }
        }

        private void LoadLevelJsonFromResources(int levelIndex1Based)
        {
            try
            {
                string path = LevelDataFileUtility.GetAbsoluteJsonPath(levelIndex1Based);
                if (!File.Exists(path))
                {
                    ShowNotification(new GUIContent($"Dosya yok — Level_{levelIndex1Based}.json"));
                    return;
                }

                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<LevelData>(json);
                if (data == null || data.hooks == null || data.hooks.Length == 0)
                {
                    ShowNotification(new GUIContent("Geçersiz level JSON."));
                    return;
                }

                ApplyLevelGeneratorLevelData(data);
                Repaint();
                ShowNotification(new GUIContent($"Yüklendi — Level_{levelIndex1Based}.json"));
            }
            catch (Exception e)
            {
                ShowNotification(new GUIContent($"Yükleme hatası\n{e.Message}"));
            }
        }

        private void ApplyLevelGeneratorLevelData(LevelData data)
        {
            _stickCount = Mathf.Clamp(data.stickCount, 1, MaxStickCount);

            for (int i = 0; i < _stickCount; i++)
            {
                _stickColorTypes[i] = data.stickColorTypes != null && i < data.stickColorTypes.Length
                    ? ParseColorTypeFromJson(data.stickColorTypes, i)
                    : ColorType.Red;
            }

            int hookCount = Mathf.Clamp(data.hookCount, 1, MaxHookCount);
            if (data.hooks.Length < hookCount)
                hookCount = data.hooks.Length;
            _hookCount = Mathf.Max(1, hookCount);

            for (int h = 0; h < _hookCount; h++)
            {
                HookLevelData hook = data.hooks[h];
                if (hook == null)
                    continue;

                _baseRingCountPerHook[h] = Mathf.Clamp(hook.baseRingCount, MinBaseRing, MaxBaseRing);
                _forkPerHook[h] = hook.fork;
                _forkBranch1BaseRingCountPerHook[h] = Mathf.Clamp(hook.branch1BaseRingCount, MinBaseRing, MaxBaseRing);
                _forkBranch2BaseRingCountPerHook[h] = Mathf.Clamp(hook.branch2BaseRingCount, MinBaseRing, MaxBaseRing);

                int trunk = _baseRingCountPerHook[h];
                for (int i = 0; i < trunk; i++)
                {
                    _ringColorTypePerHook[h, RingSegmentTrunk, i] = ParseColorTypeFromJson(
                        hook.trunkRingColorTypes,
                        i);
                }

                int b1 = _forkBranch1BaseRingCountPerHook[h];
                int b2 = _forkBranch2BaseRingCountPerHook[h];

                if (hook.fork)
                {
                    for (int i = 0; i < b1; i++)
                        _ringColorTypePerHook[h, RingSegmentBranch1, i] = ParseColorTypeFromJson(hook.branch1RingColorTypes, i);
                    for (int i = 0; i < b2; i++)
                        _ringColorTypePerHook[h, RingSegmentBranch2, i] = ParseColorTypeFromJson(hook.branch2RingColorTypes, i);
                }
            }
        }

        private ColorType ParseColorTypeFromJson(byte[] bytes, int index)
        {
            if (bytes == null || index < 0 || index >= bytes.Length)
                return ColorType.Red;

            byte value = bytes[index];
            int enumLength = System.Enum.GetValues(typeof(ColorType)).Length;
            if (value >= enumLength)
                return ColorType.Red;
            return (ColorType)value;
        }

        private void DrawJsonExportSection()
        {
            EditorGUILayout.LabelField("JSON", EditorStyles.boldLabel);

            string json = BuildLevelGeneratorJson();
            float contentWidth = EditorGUIUtility.currentViewWidth - 48f;
            if (contentWidth < 80f)
                contentWidth = 200f;

            GUIStyle textStyle = EditorStyles.textArea;
            float contentHeight = textStyle.CalcHeight(new GUIContent(json), contentWidth);
            contentHeight = Mathf.Max(contentHeight + 8f, JsonPanelViewportHeight);

            _jsonScrollPosition = EditorGUILayout.BeginScrollView(
                _jsonScrollPosition,
                GUI.skin.box,
                GUILayout.Height(JsonPanelViewportHeight));

            EditorGUILayout.SelectableLabel(
                json,
                textStyle,
                GUILayout.Width(contentWidth),
                GUILayout.Height(contentHeight));

            EditorGUILayout.EndScrollView();
        }

        private string BuildLevelGeneratorJson()
        {
            var stickColors = new byte[_stickCount];
            for (int i = 0; i < _stickCount; i++)
                stickColors[i] = (byte)_stickColorTypes[i];

            var root = new LevelData
            {
                stickCount = _stickCount,
                hookCount = _hookCount,
                stickColorTypes = stickColors,
                hooks = new HookLevelData[_hookCount]
            };

            for (int h = 0; h < _hookCount; h++)
            {
                int trunk = _baseRingCountPerHook[h];
                bool fork = _forkPerHook[h];
                int b1 = fork ? _forkBranch1BaseRingCountPerHook[h] : 0;
                int b2 = fork ? _forkBranch2BaseRingCountPerHook[h] : 0;

                var trunkColors = new byte[trunk];
                for (int i = 0; i < trunk; i++)
                    trunkColors[i] = (byte)_ringColorTypePerHook[h, RingSegmentTrunk, i];

                root.hooks[h] = new HookLevelData
                {
                    baseRingCount = trunk,
                    fork = fork,
                    branch1BaseRingCount = b1,
                    branch2BaseRingCount = b2,
                    trunkRingColorTypes = trunkColors,
                    branch1RingColorTypes = fork ? BuildRingColorByteArray(h, RingSegmentBranch1, b1) : Array.Empty<byte>(),
                    branch2RingColorTypes = fork ? BuildRingColorByteArray(h, RingSegmentBranch2, b2) : Array.Empty<byte>()
                };
            }

            return JsonUtility.ToJson(root, true);
        }

        private byte[] BuildRingColorByteArray(int hookIndex, int segment, int count)
        {
            var arr = new byte[count];
            for (int i = 0; i < count; i++)
                arr[i] = (byte)_ringColorTypePerHook[hookIndex, segment, i];
            return arr;
        }
    }
}
