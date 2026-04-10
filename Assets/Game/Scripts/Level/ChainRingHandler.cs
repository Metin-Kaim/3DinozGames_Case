using System.Collections.Generic;
using UnityEngine;

public class ChainRingHandler : MonoBehaviour
{
    [SerializeField] private ChainRingHandler upperRing;
    [SerializeField] private List<ChainRingHandler> lowerRings = new();

    private Rigidbody _rigidbody;
    private FixedJoint _fixedJoint;

    public Rigidbody Rb => _rigidbody;

    public ChainRingHandler UpperRing => upperRing;
    public List<ChainRingHandler> LowerRings => lowerRings;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _fixedJoint = GetComponent<FixedJoint>();
    }

    public void ConnectAbove(ChainRingHandler upper)
    {
        upperRing = upper;
        upper?.lowerRings.Add(this);

        if (Rb == null || _fixedJoint == null)
            return;

        if (upper != null)
        {
            _fixedJoint.connectedBody = upper.Rb;
            Rb.isKinematic = false;
        }
        else
        {
            _fixedJoint.connectedBody = null;
            Rb.isKinematic = true;
        }
    }
}
