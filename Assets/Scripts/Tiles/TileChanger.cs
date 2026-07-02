using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TileChanger : UdonSharpBehaviour
{
    public TileType TargetTile;
    [SerializeField] private GameObject box;
    [SerializeField] private GameObject spawner;
    [SerializeField] private VRC_Pickup pickup;

    [UdonSynced] private bool _active;
    private VRCPlayerApi _currentMasterPlayer;
    private Vector3 _startingPosition;

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

    public void Init(VRCPlayerApi currentMasterPlayer, TileType targetTile, Transform location)
    {
        Networking.SetOwner(currentMasterPlayer, gameObject);
        Debug.Log($"Setting target tile {targetTile}");
        TargetTile = targetTile;
        _active = false;
        UpdateActiveState();
        transform.position = location.position;
    }

    private void Awake()
    {
        _startingPosition = transform.position;
        gameObject.SetActive(false);
    }

    private void Start()
    {
        if (_currentMasterPlayer == null)
        {
            _currentMasterPlayer = Networking.GetOwner(gameObject);
        }

        pickup.pickupable = _currentMasterPlayer.isLocal;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_active) return;
        
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        if (tile.TryChangeTileType(TargetTile))
        {
            transform.position = _startingPosition;
            VRCTween.DelayedSetActive(gameObject, false, 2f);
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
        if (_active)
        {
            pickup.UseText = GetDescription();
        }
        else
        {
            pickup.UseText = $"Use to Activate: ({GetDescription()})";
        }
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
