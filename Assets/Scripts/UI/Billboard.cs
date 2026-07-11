using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Billboard : UdonSharpBehaviour
{
    [SerializeField] private Transform billboard;
    
    private void LateUpdate()
    {
        Vector3 targetPosition = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        billboard.LookAt(targetPosition);
    }
}
