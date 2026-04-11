using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Game.Scripts.Editor
{
    internal static class LevelDataFileUtility
    {
        public static string GetAbsoluteJsonPath(int levelIndex1Based)
        {
            return Path.Combine(Application.dataPath, "Resources", "LevelDatas", $"Level_{levelIndex1Based}.json");
        }

        public static void EnsureDirectoryExists()
        {
            string dir = Path.Combine(Application.dataPath, "Resources", "LevelDatas");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static int[] ScanExistingLevelIndices()
        {
            string dir = Path.Combine(Application.dataPath, "Resources", "LevelDatas");
            if (!Directory.Exists(dir))
                return System.Array.Empty<int>();

            var list = new List<int>();
            foreach (string path in Directory.GetFiles(dir, "Level_*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.Length > 6
                    && name.StartsWith("Level_", System.StringComparison.Ordinal)
                    && int.TryParse(name.Substring(6), out int n)
                    && n >= 1)
                {
                    list.Add(n);
                }
            }

            list.Sort();
            return list.ToArray();
        }
    }
}
