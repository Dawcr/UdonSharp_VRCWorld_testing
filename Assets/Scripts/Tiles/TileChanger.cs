using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TileChanger : UdonSharpBehaviour
{
    public TileType TargetTile;
    [SerializeField] private GameObject box;
    [SerializeField] private GameObject spawner;
    [SerializeField] private VRC_Pickup pickup;
    [SerializeField] private Rigidbody rb;

    [UdonSynced] private bool _active;
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

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        pickup.pickupable = player.isLocal;
    }

    public void Spawn(VRCPlayerApi currentMasterPlayer, TileType targetTile, Transform location)
    {
        Networking.SetOwner(currentMasterPlayer, gameObject);
        Debug.Log($"Setting target tile {targetTile}");
        TargetTile = targetTile;
        transform.position = location.position;
        SetInactive();
    }

    private void SetInactive()
    {
        _active = false;
        UpdateActiveState();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    [NetworkCallable]
    public void Respawn()
    {
        pickup.Drop();
        transform.position = _startingPosition;
        SetInactive();
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

    private void OnTriggerEnter(Collider other)
    {
        if (!_active) return;
        if (!Networking.IsOwner(gameObject)) return;
        
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        if (tile.TryChangeTileType(TargetTile))
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Respawn));
        }
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

    private string GetDescription()
    {
        switch (TargetTile)
        {
            case TileType.None:
                return "Remove current tile";
            default:
                return "Testing";
        }
    }
}
