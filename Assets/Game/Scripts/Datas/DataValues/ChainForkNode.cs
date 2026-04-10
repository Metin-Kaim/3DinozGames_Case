using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChainForkNode
{
    [Tooltip("Fork öncesi (bu segmentte) üst üste kaç halka.")]
    [Min(1)]
    public int baseRingCount = 3;

    [Tooltip("Boşsa fork yok. Her eleman fork sonrası bir dal (içinde kendi baseRingCount ve alt dalları).")]
    public List<ChainForkNode> branches = new();
}
