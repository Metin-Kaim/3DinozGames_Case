using System.Collections.Generic;
using Assets.Game.Scripts.Datas.UnityValues;
using UnityEngine;

namespace Assets.Game.Scripts.Editor
{
    /// <summary>
    /// <see cref="LevelGenerator"/> ile aynı grid mantığı; önizleme rect içinde GUI merkezleri üretir.
    /// </summary>
    public class StickLayoutPreviewGeometry
    {
        public const float LayoutSpacingX = 1.5f;
        public const float LayoutSpacingY = 1.5f;

        public static bool TryGetStickGuiCenters(
            Rect previewRect,
            int stickCount,
            StickLayoutConfig config,
            out Vector2[] guiCenters)
        {
            guiCenters = null;
            if (config == null || stickCount <= 0)
                return false;

            Vector2[] layout = BuildLayoutPositions(stickCount, config);
            if (layout == null || layout.Length != stickCount)
                return false;

            guiCenters = MapLayoutToGui(previewRect, layout);
            return guiCenters != null;
        }

        public static bool TryHitStickIndex(
            Rect previewRect,
            Vector2 mouse,
            int stickCount,
            StickLayoutConfig config,
            float hitRadius,
            out int stickIndex)
        {
            stickIndex = -1;
            if (!TryGetStickGuiCenters(previewRect, stickCount, config, out Vector2[] centers))
                return false;

            for (int i = 0; i < centers.Length; i++)
            {
                if (Vector2.Distance(mouse, centers[i]) <= hitRadius)
                {
                    stickIndex = i;
                    return true;
                }
            }

            return false;
        }

        private static Vector2[] BuildLayoutPositions(int stickCount, StickLayoutConfig config)
        {
            Vector2Int grid = config.GetGrid(stickCount);
            int row = grid.x;
            int col = grid.y;
            float yCenterOffset = row > 1 ? (row - 1) * LayoutSpacingY * 0.5f : 0f;
            int index = 0;

            var list = new List<Vector2>(stickCount);

            for (int r = 0; r < row; r++)
            {
                int itemsInRow = Mathf.Min(col, stickCount - index);
                float startX = -(itemsInRow - 1) * LayoutSpacingX / 2f;

                for (int c = 0; c < itemsInRow; c++)
                {
                    float x = startX + c * LayoutSpacingX;
                    float y = yCenterOffset - r * LayoutSpacingY;
                    list.Add(new Vector2(x, y));
                    index++;
                    if (index >= stickCount)
                        return list.ToArray();
                }
            }

            return list.ToArray();
        }

        private static Vector2[] MapLayoutToGui(Rect previewRect, Vector2[] layout)
        {
            if (layout == null || layout.Length == 0)
                return null;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (Vector2 p in layout)
            {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }

            float w = Mathf.Max(maxX - minX, 0.01f);
            float h = Mathf.Max(maxY - minY, 0.01f);
            const float pad = 20f;
            float availW = Mathf.Max(previewRect.width - pad * 2f, 1f);
            float availH = Mathf.Max(previewRect.height - pad * 2f, 1f);

            // Tek satır / tek sütun gibi durumlarda w veya h çok küçük kalıyor; Min(avail/w, avail/h)
            // sadece ince eksene göre devasa ölçek üretip çubuğu yatayda kenarlara yapıştırıyor.
            // max(w,h) ile tek ölçek — 2–3 stick satırı da 4–5 gibi önizleme içinde ortalanır.
            float extent = Mathf.Max(w, h);
            float scale = Mathf.Min(availW / extent, availH / extent);

            Vector2 layoutCenter = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            var gui = new Vector2[layout.Length];

            for (int i = 0; i < layout.Length; i++)
            {
                Vector2 d = layout[i] - layoutCenter;
                gui[i] = new Vector2(
                    previewRect.center.x + d.x * scale,
                    previewRect.center.y - d.y * scale);
            }

            return gui;
        }
    }
}
