using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
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
                Debug.LogError("Unknown GatherableResourceType");
                return defaultLocation;
        }
    }
}
