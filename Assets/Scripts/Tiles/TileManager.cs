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
    [SerializeField] private float tileXSize;
    [SerializeField] private float tileZSize;

    [UdonSynced] private byte[] _worldTilesType;
    [UdonSynced] private byte[] _worldTilesRotation;
    private Tile[] _worldTilesObject;
    private const string WorldTilesStringName = "WorldTiles";
    private const string WorldTilesRotationStringName = "WorldTilesRotation";
    private int _maxXTiles;
    private int _maxZTiles;
    private int _maxTiles;
    private float _startingXLocation;
    private float _startingZLocation;
    // is this necessary here?
    private bool _restoreComplete;
    
    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        
        SetWorldTiles();
        SpawnInTiles();
        _restoreComplete = true;
    }
    
    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        foreach (Tile tile in _worldTilesObject)
        {
            if (tile != null)
            {
                Destroy(tile.gameObject);
            }
        }

        if (!player.isLocal)
        {
            SendCustomEventDelayedSeconds(nameof(SpawnInTiles), 5f);
            return;
        }
        
        SetWorldTiles();
        SpawnInTiles();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        RequestSerialization();
    }

    public void ChangeTile(int tileIndex, TileType tileType)
    {
        _worldTilesType[tileIndex] = (byte)tileType;
        RequestSerialization();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ChangeTilePrefab), tileIndex, (byte)tileType);
        SaveTileTypes();
    }

    public void RotateTile(int tileIndex)
    {
        _worldTilesRotation[tileIndex] += 1; // 1 * 90 degrees clockwise
        RequestSerialization();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(RotateTileObject), tileIndex);
        SaveTileRotations();
    }

    [NetworkCallable]
    public void ChangeTilePrefab(int tileIndex, byte tileType)
    {
        if (_worldTilesObject[tileIndex] == null)
        {
            Debug.Log("Tile not found");
            return;
        }
        
        Vector3 tilePosition = _worldTilesObject[tileIndex].transform.position;
        Destroy(_worldTilesObject[tileIndex].gameObject);
        GameObject newTileTypeToSpawn = GetTile(tileType);
        GameObject spawnedTile = Instantiate(newTileTypeToSpawn, tilePosition, Quaternion.identity);
        Tile tile = spawnedTile.GetComponent<Tile>();
        _worldTilesObject[tileIndex] = tile;
        if (tile != null)
        {
            tile.Init(this, tileIndex, 0);
        }
    }

    [NetworkCallable]
    public void RotateTileObject(int tileIndex)
    {
        if (_worldTilesObject[tileIndex] == null)
        {
            Debug.Log("Tile not found");
            return;
        }
        
        _worldTilesObject[tileIndex].transform.Rotate(0, 90, 0);
    }
    
    public void SpawnInTiles()
    {
        Debug.Log("Spawning tiles");
        for (int x = 0; x < _maxXTiles; x++)
        {
            for (int z = 0; z < _maxZTiles; z++)
            {
                Vector3 location = new Vector3(_startingXLocation - x * tileXSize, 0, _startingZLocation - z * tileZSize);
                int index = (x * _maxZTiles) + z;
                GameObject tileToSpawn = GetTile(_worldTilesType[index]);
                GameObject spawnedTile = Instantiate(tileToSpawn, location, Quaternion.identity);
                Tile newTile = spawnedTile.GetComponent<Tile>();
                _worldTilesObject[index] = newTile;
                float rotation = (_worldTilesRotation[index] % 4) * 90f;
                if (newTile != null)
                {
                    newTile.Init(this, index, rotation);
                }
            }
        }
    }

    private void Start()
    {
        Init();
        HandleNullValues();
    }

    private void HandleNullValues()
    {
        if (worldFloor == null)
        {
            Debug.LogError("World floor in TileManager script is null");
        }

        if (baseTilePrefab == null)
        {
            Debug.LogError("BaseTilePrefab in TileManager script is null");
        }

        if (grassyWoodenBenchPrefab == null)
        {
            Debug.LogError("GrassyWoodenBenchPrefab in TileManager script is null");
        }
    }

    private void Init()
    {
        float worldScaleX = worldFloor.transform.localScale.x;
        // whyy
        float worldScaleZ = worldFloor.transform.localScale.y;
        
        _maxXTiles = Mathf.FloorToInt(worldScaleX / tileXSize);
        _maxZTiles = Mathf.FloorToInt(worldScaleZ / tileZSize);
        _maxTiles = _maxXTiles * _maxZTiles;
        
        //Debug.Log($"Max tiles: {_maxXTiles}x{_maxZTiles}");

        _startingXLocation =
            worldFloor.transform.localPosition.x + (worldScaleX - tileXSize) / 2;
        _startingZLocation =
            worldFloor.transform.localPosition.z + (worldScaleZ - tileZSize) / 2;
        
        //Debug.Log($"Starting locations at {_startingXLocation}, {_startingZLocation}");
        
        _worldTilesObject = new Tile[_maxTiles];
    }

    private void SetWorldTiles()
    {
        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        if (!owner.isLocal) return;
        
        if (!PlayerData.TryGetBytes(owner, WorldTilesStringName, out _worldTilesType))
        {
            Debug.Log("No playerData for TileTypes recovered");
            _worldTilesType = new byte[_maxTiles];
        }

        if (!PlayerData.TryGetBytes(owner, WorldTilesRotationStringName, out _worldTilesRotation))
        {
            Debug.Log("No playerData for TileRotations recovered");
            _worldTilesRotation = new byte[_maxTiles];
        }
        
        RequestSerialization();
    }

    private void SaveTileTypes()
    {
        if (!_restoreComplete) return;
        if (!Networking.IsOwner(gameObject)) return;
        
        PlayerData.SetBytes(WorldTilesStringName, _worldTilesType);
        Debug.Log("Tiles types saved");
    }

    private void SaveTileRotations()
    {
        if (!_restoreComplete) return;
        if (!Networking.IsOwner(gameObject)) return;
        
        PlayerData.SetBytes(WorldTilesRotationStringName, _worldTilesRotation);
        Debug.Log("Tile rotations saved");
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
