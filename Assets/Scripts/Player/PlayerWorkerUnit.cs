using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDK3.Components;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerWorkerUnit : UdonSharpBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private Transform inventoryStorageLocation;
    [SerializeField] private float gatherTimeMultiplier = 1f;
    [SerializeField] private GatherableResourceData gatherableResourceData;
    private NavMeshAgent _agent;
    private TMP_Text _playerName;
    private GatherableResourceType _gatheredResource;
    private VRCTweenHandle _gatherOperation;
    private Transform _gatherTarget;
    private bool _isHoldingResource;
    // I'm not sure if I'll want it to be incremental
    private const int GatheredResourceAmount = 1;

    public void GoGather(GatherableResourceType resourceType)
    {
        _gatherTarget = gatherableResourceData.GetResourceLocation(resourceType);
        SetTarget(_isHoldingResource ? inventoryStorageLocation : _gatherTarget);
    }
    
    // there must be a better way to do this..?
    public void ReceiveResource(GatherableResourceType resource, float gatherTime)
    {
        if (!Networking.IsOwner(gameObject)) return;
        
        float finalGatherTime = gatherTime * gatherTimeMultiplier;
        _gatheredResource = resource;
        Debug.Log($"Starting to gather {GatherableResourceTypeExtensions.GetName(_gatheredResource)}");
        _gatherOperation = VRCTween.DelayedCall(this, nameof(StoreCurrentResource), finalGatherTime);
    }

    public void StoreCurrentResource()
    {
        if (!Networking.IsOwner(gameObject)) return;
        
        _isHoldingResource = true;
        Debug.Log("Walking to storage");
        SetTarget(inventoryStorageLocation);
    }
    
    public void DropResource()
    {
        if (!Networking.IsOwner(gameObject)) return;
        if (!_isHoldingResource) return;
        
        Debug.Log($"Storing {GatheredResourceAmount} {GatherableResourceTypeExtensions.GetName(_gatheredResource)}");
        inventory.AddItems(_gatheredResource, GatheredResourceAmount);
        _isHoldingResource = false;
        Debug.Log($"Walking to {GatherableResourceTypeExtensions.GetName(_gatheredResource)}");
        SetTarget(_gatherTarget);
    }
    
    private void Start()
    {
        Init();
        HandleNullValues();
    }

    private void HandleNullValues()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory in PlayerWorkerUnit is null");
        }

        if (inventoryStorageLocation == null)
        {
            Debug.LogError("InventoryStorageLocation in PlayerWorkerUnit is null");
        }

        if (gatherableResourceData == null)
        {
            Debug.LogError("GatherableResourceData in PlayerWorkerUnit is null");
        }
    }

    private void Init()
    {
        _agent = GetComponent<NavMeshAgent>();
        _playerName = GetComponentInChildren<TMP_Text>();
        _playerName.text = Networking.GetOwner(gameObject).displayName;
        _agent.SetDestination(transform.position);
    }

    private void SetTarget(Transform target)
    {
        if (_gatherOperation.IsActive)
        {
            _gatherOperation.Kill();
        }
        
        if (target != null)
        {
            _agent.SetDestination(target.position);
        }
    }
}
