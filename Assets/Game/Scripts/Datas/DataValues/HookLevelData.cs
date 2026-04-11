using System;

namespace Assets.Game.Scripts.Datas.DataValues
{
    [Serializable]
    public class HookLevelData
    {
        public int baseRingCount;
        public bool fork;
        public int branch1BaseRingCount;
        public int branch2BaseRingCount;
        public byte[] trunkRingColorTypes;
        public byte[] branch1RingColorTypes;
        public byte[] branch2RingColorTypes;
    }
}