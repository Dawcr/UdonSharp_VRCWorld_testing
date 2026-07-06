using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TileManager : UdonSharpBehaviour
{
    [SerializeField] private GameObject worldFloor;
    [SerializeField] private GameObject baseTilePrefab;
    [SerializeField] private GameObject grassyWoodenBenchPrefab;

    [UdonSynced] private byte[] _worldTiles;
    private GameObject[] _worldTilesGameObjects;
    private byte[] _localTiles;
    private const string WorldTilesStringName = "WorldTiles";
    private int _maxXTiles;
    private int _maxZTiles;
    private int _maxTiles;
    private float _tileXSize;
    private float _tileZSize;
    private float _startingXLocation;
    private float _startingZLocation;
    private float _worldScaleX;
    private float _worldScaleZ;
    // is this necessary here?
    private bool _restoreComplete;
    
    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        if (!player.isLocal) return;

        SetWorldTiles(Networking.GetOwner(gameObject));
        SpawnInTiles();
        _restoreComplete = true;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        RequestSerialization();
    }

    public void ChangeTile(int tileIndex, TileType tileType)
    {
        _worldTiles[tileIndex] = (byte)tileType;
        RequestSerialization();
        //ChangeTilePrefab(tileIndex, tileType);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ChangeTilePrefab), tileIndex, (byte)tileType);
        SaveTiles();
    }

    [NetworkCallable]
    public void ChangeTilePrefab(int tileIndex, byte tileType)
    {
        if (_worldTilesGameObjects[tileIndex] == null)
        {
            Debug.Log("Tile not found");
            return;
        }
        
        Vector3 tilePosition = _worldTilesGameObjects[tileIndex].transform.position;
        Destroy(_worldTilesGameObjects[tileIndex]);
        GameObject newTileTypeToSpawn = GetTile(tileType);
        GameObject spawnedTile = Instantiate(newTileTypeToSpawn, tilePosition, Quaternion.identity);
        _worldTilesGameObjects[tileIndex] = spawnedTile;
        Tile tile = spawnedTile.GetComponent<Tile>();
        if (tile != null)
        {
            tile.Init(this, tileIndex);
        }
    }
    
    /*
    public override void OnDeserialization()
    {
        for (int i = 0; i < _maxXTiles; i++)
        {
            if (_localTiles[i] != _worldTiles[i])
            {
                Debug.Log($"Changing tile at index {i}");
                _localTiles[i] = _worldTiles[i];
                ChangeTilePrefab(i, _worldTiles[i]);
            }
        }
    }
    */

    private void SetWorldTiles(VRCPlayerApi player)
    {
        if (!PlayerData.TryGetBytes(player, WorldTilesStringName, out _worldTiles))
        {
            Debug.Log("No playerData recovered");
            _worldTiles = new byte[_maxTiles];
        }

        _worldTiles.CopyTo(_localTiles, 0);
        RequestSerialization();
    }

    private void Start()
    {
        _worldScaleX = worldFloor.transform.localScale.x;
        // whyy
        _worldScaleZ = worldFloor.transform.localScale.y;
        
        _maxXTiles = Mathf.FloorToInt(_worldScaleX / baseTilePrefab.transform.localScale.x);
        _maxZTiles = Mathf.FloorToInt(_worldScaleZ / baseTilePrefab.transform.localScale.z);
        _maxTiles = _maxXTiles * _maxZTiles;
        
        Debug.Log($"Max tiles: {_maxXTiles}x{_maxZTiles}");

        _tileXSize = baseTilePrefab.transform.localScale.x;
        _tileZSize = baseTilePrefab.transform.localScale.z;
        
        Debug.Log($"Tile size: {_tileXSize}x{_tileZSize}");

        _startingXLocation =
            worldFloor.transform.localPosition.x + (_worldScaleX - _tileXSize) / 2;
        _startingZLocation =
            worldFloor.transform.localPosition.z + (_worldScaleZ - _tileZSize) / 2;
        
        Debug.Log($"Starting locations at {_startingXLocation}, {_startingZLocation}");
        
        _localTiles = new byte[_maxTiles];
        _worldTilesGameObjects = new GameObject[_maxTiles];
    }
    
    private void SpawnInTiles()
    {
        Debug.Log("Spawning tiles");
        for (int x = 0; x < _maxXTiles; x++)
        {
            for (int z = 0; z < _maxZTiles; z++)
            {
                Vector3 location = new Vector3(_startingXLocation - x * _tileXSize, 0, _startingZLocation - z * _tileZSize);
                int index = (x * _maxZTiles) + z;
                GameObject tileToSpawn = GetTile(_worldTiles[index]);
                GameObject spawnedTile = Instantiate(tileToSpawn, location, Quaternion.identity);
                _worldTilesGameObjects[index] = spawnedTile.gameObject;
                Tile newTile = spawnedTile.GetComponent<Tile>();
                if (newTile != null)
                {
                    newTile.Init(this, index);
                }
            }
        }
    }

    private void SaveTiles()
    {
        if (!_restoreComplete) return;
        if (!Networking.IsOwner(gameObject)) return;
        
        PlayerData.SetBytes(WorldTilesStringName, _worldTiles);
        Debug.Log("Tiles saved");
    }

    private GameObject GetTile(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.GrassyWoodenBench:
                return grassyWoodenBenchPrefab;
            default:
                return baseTilePrefab;
        }
    }

    private GameObject GetTile(byte tile)
    {
        return GetTile((TileType)tile);
    }
}
