using UdonSharp;
using UnityEngine;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public abstract class TileTool : UdonSharpBehaviour
{
    [SerializeField] private GameObject box;
    [SerializeField] private GameObject spawner;
    [SerializeField] private VRC_Pickup pickup;
    [SerializeField] private Rigidbody rb;
    
    [UdonSynced] protected bool _active;
    private readonly Vector3 _startingPosition = new Vector3(0, -20, 0);

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        UpdateActiveState();
    }

    public override void OnPickupUseDown()
    {
        _active = !_active;
        RequestSerialization();
        UpdateActiveState();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi newOwner)
    {
        pickup.pickupable = newOwner.isLocal;
    }

    [NetworkCallable]
    public void Respawn()
    {
        pickup.Drop();
        transform.position = _startingPosition;
        SetInactive();
    }
    
    protected abstract string GetDescription();
    
    protected void SetInactive()
    {
        _active = false;
        UpdateActiveState();
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    
    private void Awake()
    {
        transform.position = _startingPosition;
    }

    private void Start()
    {
        HandleNullValues();
        pickup.AutoHold = Networking.LocalPlayer.IsUserInVR()
            ? VRC_Pickup.AutoHoldMode.No
            : VRC_Pickup.AutoHoldMode.Yes;
    }

    private void HandleNullValues()
    {
        if (box == null)
        {
            Debug.LogError($"Box in TileChanger is null");
        }

        if (spawner == null)
        {
            Debug.LogError($"Spawner in TileChanger is null");
        }

        if (pickup == null)
        {
            Debug.LogWarning($"Pickup in TileChanger is null, trying to GetComponent");
            pickup = GetComponent<VRC_Pickup>();
        }

        if (rb == null)
        {
            Debug.LogWarning($"Rigidbody in TileChanger is null, trying to GetComponent");
            rb = GetComponent<Rigidbody>();
        }
    }

    private void UpdateActiveState()
    {
        box.SetActive(!_active);
        spawner.SetActive(_active);
        UpdateSpawnerDescription();
    }

    private void UpdateSpawnerDescription()
    {
        pickup.UseText = _active ? GetDescription() : $"Use to Activate: ({GetDescription()})";
    }
    
}
