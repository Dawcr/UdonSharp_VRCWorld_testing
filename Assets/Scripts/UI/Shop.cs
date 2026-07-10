using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Shop : UdonSharpBehaviour
{
    [SerializeField] private GameObject tileChanger;
    [SerializeField] private PlayerInventory playerInventory;

    private readonly int[] _tileDestroyerPrice = { 1, 1 };
    private readonly int[] _testTilePrice = { 10, 5 };
    
    public void SpawnTileDestroyer()
    {
        if (!playerInventory.TryToPay(_tileDestroyerPrice)) return;
        SpawnTileChanger(TileType.None);
    }

    public void SpawnTestTile()
    {
        if (!playerInventory.TryToPay(_testTilePrice)) return;
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

    private void Awake()
    {
    }
}
