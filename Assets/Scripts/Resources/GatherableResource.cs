using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class GatherableResource : UdonSharpBehaviour
{
    [SerializeField] private GatherableResourceType gatherableResourceType;
    [SerializeField] private float gatherTime = 2f;

    private void OnTriggerEnter(Collider other)
    {
        //if (!Networking.IsOwner(other.gameObject)) return;
        
        PlayerWorkerUnit worker = other.GetComponent<PlayerWorkerUnit>();
        if (worker == null) return;
        
        worker.ReceiveResource(gatherableResourceType, gatherTime);
    }
}