using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ManualObjectSync : UdonSharpBehaviour
{
    [SerializeField] private Rigidbody rb;
    [UdonSynced] private Vector3 _position;
    [UdonSynced] private Quaternion _rotation;
    private const float SyncInterval = 0.11f;
    private VRCTweenHandle _tween;
    private bool _wasKinematic;
    
    public void Sync()
    {
        if (!Networking.IsOwner(gameObject)) return;
        _position = transform.position;
        _rotation = transform.rotation;
        RequestSerialization();
        _tween = VRCTween.DelayedCall(this, nameof(Sync), SyncInterval);
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        SetRemoteKinematic(player);
        _tween = VRCTween.DelayedCall(this, nameof(Sync), SyncInterval);
    }

    public override void OnDeserialization()
    {
        transform.position = _position;
        transform.rotation = _rotation;
    }

    private void SetRemoteKinematic(VRCPlayerApi player)
    {
        rb.isKinematic = _wasKinematic || !player.isLocal;
    }

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        _wasKinematic = rb.isKinematic;
    }

    private void Start()
    {
        SetRemoteKinematic(Networking.GetOwner(gameObject));
    }
    
    private void OnEnable()
    { 
        _tween = VRCTween.DelayedCall(this, nameof(Sync), SyncInterval);
    }

    private void OnDisable()
    {
        _tween.Kill();
    }
}
