using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GatherableResource : UdonSharpBehaviour
{
    [SerializeField] private GatherableResourceType gatherableResourceType;
    [SerializeField] private float gatherTime = 2f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerWorkerUnit worker = other.GetComponent<PlayerWorkerUnit>();
        if (worker == null) return;
        
        worker.ReceiveResource(gatherableResourceType, gatherTime);
    }
}