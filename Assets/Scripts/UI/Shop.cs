using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Shop : UdonSharpBehaviour
{
    [SerializeField] private TileChanger tileChanger;
    [SerializeField] private TileRotator tileRotator;
    [SerializeField] private PlayerInventory playerInventory;

    private readonly int[] _tileDestroyerPrice = { 1, 1 };
    private readonly int[] _testTilePrice = { 10, 5 };
    
    public void SpawnTileDestroyer()
    {
        if (!playerInventory.TryToPay(_tileDestroyerPrice)) return;
        RespawnTileChanger(TileType.None);
    }

    public void SpawnTestTile()
    {
        if (!playerInventory.TryToPay(_testTilePrice)) return;
        RespawnTileChanger(TileType.GrassyWoodenBench);
    }

    public void SpawnTileRotator()
    {
        RespawnTileRotator();
    }

    private void Start()
    {
        HandleNullValues();
    }

    private void HandleNullValues()
    {
        if (tileChanger == null)
        {
            Debug.LogError($"TileChanger in Shop script is null");
        }

        if (playerInventory == null)
        {
            Debug.LogError($"PlayerInventory in Shop script is null");
        }
    }

    private void RespawnTileChanger(TileType tileType)
    {
        tileChanger.Spawn(Networking.GetOwner(gameObject), tileType, transform);
    }

    private void RespawnTileRotator()
    {
        tileRotator.Spawn(Networking.GetOwner(gameObject), transform);
    }
}
