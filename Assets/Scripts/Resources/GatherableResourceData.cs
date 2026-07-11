using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GatherableResourceData : UdonSharpBehaviour
{
    [SerializeField] private Transform defaultLocation;
    [SerializeField] private Transform woodLocation;
    [SerializeField] private Transform stoneLocation;
    
    public Transform GetResourceLocation(GatherableResourceType resourceType)
    {
        Debug.Log($"SetGatherLocation{GatherableResourceTypeExtensions.GetName(resourceType)}");
        switch (resourceType)
        {
            case GatherableResourceType.Wood:
                return woodLocation;
            case GatherableResourceType.Stone:
                return stoneLocation;
            default:
                Debug.LogError("Unknown GatherableResourceType passed in");
                return defaultLocation;
        }
    }

    private void Start()
    {
        HandleNullValues();
    }

    private void HandleNullValues()
    {
        if (defaultLocation == null)
        {
            Debug.LogError("DefaultLocation in GatherableResourceData is null");
        }

        if (woodLocation == null)
        {
            Debug.LogError("WoodLocation in GatherableResourceData is null");
        }

        if (stoneLocation == null)
        {
            Debug.LogError("StoneLocation in GatherableResourceData is null");
        }
    }
}
