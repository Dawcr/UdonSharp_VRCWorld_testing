using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Tile : UdonSharpBehaviour
{
    [SerializeField] private TileType tileType;
    private TileManager _tileManager;
    private int _index;

    public void Init(TileManager tileManager, int index, float rotation)
    {
        _tileManager = tileManager;
        _index = index;
        if (rotation != 0)
        {
            transform.Rotate(0, rotation, 0);
        }
    }
    
    public bool TryChangeTileType(TileType newTileType)
    {
        if (newTileType == tileType) return false;
        // only support demolishing and building on an empty tile
        if (tileType != TileType.None && newTileType != TileType.None) return false;
        
        tileType = newTileType;
        SendCustomEventDelayedFrames(nameof(GetReplaced), 0);
        return true;
    }

    public bool Rotate90Degrees()
    {
        if (tileType == TileType.None) return false;
           
        _tileManager.RotateTile(_index);
        return true;
    }
    
    public void GetReplaced()
    {
        if (_tileManager == null) return;
        
        _tileManager.ChangeTile(_index, tileType);
    }
}
