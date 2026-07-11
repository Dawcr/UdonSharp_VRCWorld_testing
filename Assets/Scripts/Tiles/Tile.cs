using System;
using TMPro;
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Tile : UdonSharpBehaviour
{
    [SerializeField] private TileType tileType;
    private TileManager _tileManager;
    private int _index;

    public void Init(TileManager tileManager, int index)
    {
        _tileManager = tileManager;
        _index = index;
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

    public void GetReplaced()
    {
        if (_tileManager == null) return;
        
        _tileManager.ChangeTile(_index, tileType);
    }
}
