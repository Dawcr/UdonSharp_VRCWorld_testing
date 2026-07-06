using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Shop : UdonSharpBehaviour
{
    [SerializeField] private GameObject tileChanger;

    public void SpawnTileDestroyer()
    {
        SpawnTileChanger(TileType.None);
    }

    public void SpawnTestTile()
    {
        SpawnTileChanger(TileType.GrassyWoodenBench);
    }
    
    private void SpawnTileChanger(TileType tileType)
    {
        TileChanger changer = tileChanger.GetComponent<TileChanger>();
        if (changer != null)
        {
            changer.Init(Networking.GetOwner(gameObject), tileType, transform);
        }
    }
}
