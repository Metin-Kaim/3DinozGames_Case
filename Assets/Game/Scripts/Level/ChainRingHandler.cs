using System.Collections.Generic;
using UnityEngine;

public class ChainRingHandler : MonoBehaviour
{
    [SerializeField] private ChainRingHandler upperRing;
    [SerializeField] private List<ChainRingHandler> lowerRings = new();

    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private FixedJoint fixedJoint;

    public Rigidbody Rb => rigidbody;

    public ChainRingHandler UpperRing => upperRing;
    public List<ChainRingHandler> LowerRings => lowerRings;

    public void ConnectAbove(ChainRingHandler upper)
    {
        upperRing = upper;
        upper?.lowerRings.Add(this);

        if (Rb == null || fixedJoint == null)
            return;

        if (upper != null)
        {
            fixedJoint.connectedBody = upper.Rb;
            Rb.isKinematic = false;
        }
        else
        {
            fixedJoint.connectedBody = null;
            Rb.isKinematic = true;
        }
    }
}
