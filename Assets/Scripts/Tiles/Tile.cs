using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Tile : UdonSharpBehaviour
{
    [SerializeField] private TileType tileType;

    private void OnTriggerEnter(Collider other)
    {
        TileChanger newTile = other.GetComponent<TileChanger>();
        if (newTile == null) return;
        // avoid changing if this tile already has content and new would replace it
        if (tileType != TileType.None && newTile.TargetTile != TileType.None) return;
        
        
    }
}
