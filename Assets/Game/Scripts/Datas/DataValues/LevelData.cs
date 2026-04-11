using System;

namespace Assets.Game.Scripts.Datas.DataValues
{
    [Serializable]
    public class LevelData
    {
        public int stickCount;
        public int hookCount;
        public byte[] stickColorTypes;
        public HookLevelData[] hooks;
    }

    
}
