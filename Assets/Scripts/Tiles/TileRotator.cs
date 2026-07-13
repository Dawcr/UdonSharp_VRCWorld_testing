using UnityEngine;
using VRC.SDKBase;

public class TileRotator : TileTool
{
    public void Spawn(VRCPlayerApi currentMasterPlayer, Transform location)
    {
        Networking.SetOwner(currentMasterPlayer, gameObject);
        transform.position = location.position;
        SetInactive();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_active) return;
        if (!Networking.IsOwner(gameObject)) return;
        
        Tile tile = other.GetComponent<Tile>();
        if (tile == null) return;

        if (tile.Rotate90Degrees())
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Respawn));
        }
    }

    protected override string GetDescription()
    {
        return "Rotate the tile 90 degrees";
    }
}
