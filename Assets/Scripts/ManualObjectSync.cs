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
    private VRCTweenHandle _syncTween;
    private bool _wasKinematic;
    private VRCTweenHandle _positionTween;
    private VRCTweenHandle _rotationTween;
    
    public void Sync()
    {
        if (!Networking.IsOwner(gameObject)) return;
        
        _position = transform.position;
        _rotation = transform.rotation;
        RequestSerialization();
        _syncTween = VRCTween.DelayedCall(this, nameof(Sync), SyncInterval);
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        SetRemoteKinematic(player);
        _syncTween = VRCTween.DelayedCall(this, nameof(Sync), SyncInterval);
    }

    public override void OnDeserialization()
    {
        if (_positionTween.IsPlaying)
        {
            _positionTween.Kill();
        }
        if (_rotationTween.IsPlaying)
        {
            _rotationTween.Kill();
        }
        
        _positionTween = gameObject.TweenPosition(_position, SyncInterval, VRCTweenEase.Linear);
        _rotationTween = gameObject.TweenRotation(_rotation.eulerAngles, SyncInterval, VRCTweenEase.Linear);
    }

    private void SetRemoteKinematic(VRCPlayerApi player)
    {
        rb.isKinematic = _wasKinematic || !player.isLocal;
    }

    private void Awake()
    {
        HandleNullValues();
        _wasKinematic = rb.isKinematic;
    }

    private void HandleNullValues()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        SetRemoteKinematic(Networking.GetOwner(gameObject));
    }
    
    private void OnEnable()
    { 
        _syncTween = VRCTween.DelayedCall(this, nameof(Sync), SyncInterval);
    }

    private void OnDisable()
    {
        _syncTween.Kill();
    }
}
