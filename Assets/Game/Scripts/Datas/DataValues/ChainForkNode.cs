using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts.Datas.DataValues
{
    [Serializable]
    public class ChainForkNode
    {
        [Tooltip("Fork öncesi (bu segmentte) üst üste kaç halka.")]
        [Range(0, 20)]
        public int baseRingCount;

        [Tooltip("Boşsa fork yok. Her eleman fork sonrası bir dal (içinde kendi baseRingCount ve alt dalları).")]
        public List<ChainForkNode> branches = new();
    }
}
