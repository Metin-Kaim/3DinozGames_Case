using Assets.Game.Scripts.Enum;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.Editor
{
    public partial class LevelGeneratorEditor
    {
        private void DrawLevelValidationSection()
        {
            EditorGUILayout.LabelField("Level doğrulama", EditorStyles.boldLabel);

            int totalRings = GetTotalRingCountInEditor();
            int minRingsRequired = _stickCount * RingsPerStick;

            // En az stick×3 halka olmalı; daha fazla halka olabilir.
            bool ringCountOk = totalRings >= minRingsRequired;
            if (!ringCountOk)
            {
                EditorGUILayout.HelpBox(
                    $"Uyarı: Toplam halka ({totalRings}), minimum gereksinimin altında ({_stickCount} × {RingsPerStick} = {minRingsRequired}). " +
                    "Zincirlerde en az bu kadar halka olmalı; stick sayısını azaltın veya halka ekleyin.",
                    MessageType.Error);
            }

            CountRingsPerColorType(out int[] ringPerColor, out int[] stickPerColor);
            int colorEnumLen = System.Enum.GetValues(typeof(ColorType)).Length;
            bool anyColorError = false;

            for (int ci = 0; ci < colorEnumLen; ci++)
            {
                int r = ringPerColor[ci];
                if (r <= 0)
                    continue;

                int cap = stickPerColor[ci] * RingsPerStick;
                if (r > cap)
                {
                    anyColorError = true;
                    var ct = (ColorType)ci;
                    EditorGUILayout.HelpBox(
                        $"Hata: {ct} — {r} halka var, bu renkte en fazla {cap} slot ({stickPerColor[ci]} stick × {RingsPerStick}). " +
                        "Stick önizlemesinde bu renge yeterli stick yok veya halka dağılımı hatalı.",
                        MessageType.Warning);
                }
            }

            if (ringCountOk && !anyColorError)
            {
                EditorGUILayout.HelpBox(
                    $"Halka sayısı minimumu sağlanıyor ({totalRings} ≥ {minRingsRequired}); renk başına slot uygun.",
                    MessageType.Info);
            }
        }

        private int GetTotalRingCountInEditor()
        {
            int total = 0;
            for (int h = 0; h < _hookCount; h++)
            {
                total += _baseRingCountPerHook[h];
                if (_forkPerHook[h])
                    total += _forkBranch1BaseRingCountPerHook[h] + _forkBranch2BaseRingCountPerHook[h];
            }

            return total;
        }

        private void CountRingsPerColorType(out int[] ringPerColor, out int[] stickPerColor)
        {
            int colorEnumLen = System.Enum.GetValues(typeof(ColorType)).Length;
            ringPerColor = new int[colorEnumLen];
            stickPerColor = new int[colorEnumLen];

            for (int h = 0; h < _hookCount; h++)
            {
                int trunk = _baseRingCountPerHook[h];
                for (int i = 0; i < trunk; i++)
                    ringPerColor[(int)_ringColorTypePerHook[h, RingSegmentTrunk, i]]++;

                if (!_forkPerHook[h])
                    continue;

                int b1 = _forkBranch1BaseRingCountPerHook[h];
                for (int i = 0; i < b1; i++)
                    ringPerColor[(int)_ringColorTypePerHook[h, RingSegmentBranch1, i]]++;

                int b2 = _forkBranch2BaseRingCountPerHook[h];
                for (int i = 0; i < b2; i++)
                    ringPerColor[(int)_ringColorTypePerHook[h, RingSegmentBranch2, i]]++;
            }

            for (int s = 0; s < _stickCount; s++)
                stickPerColor[(int)_stickColorTypes[s]]++;
        }
    }
}
