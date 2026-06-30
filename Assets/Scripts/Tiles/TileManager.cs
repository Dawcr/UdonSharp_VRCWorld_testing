using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

public class TileManager : UdonSharpBehaviour
{
    [SerializeField] private GameObject worldFloor;
    [SerializeField] private GameObject baseTilePrefab;

    private TileType[] _worldTiles;
    private const string WorldTilesStringName = "WorldTiles";
    private int _maxXTiles;
    private int _maxZTiles;
    private float _tileXSize;
    private float _tileZSize;
    private float _startingXLocation;
    private float _startingZLocation;
    private float _worldScaleX;
    private float _worldScaleZ;
    
    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        if (!Networking.IsMaster) return;

        SetWorldTiles(player);
        SpawnInTiles();
    }

    private void SetWorldTiles(VRCPlayerApi player)
    {
        if (!PlayerData.TryGetBytes(player, WorldTilesStringName, out byte[] bytes))
        {
            Debug.Log("No playerdata recovered");
            return;
        }
        if (bytes.Length < _maxXTiles * _maxZTiles)
        {
            Debug.Log($"less bytes received {bytes.Length} than world tiles available {_maxXTiles * _maxZTiles}");
            return;
        }
        
        for (int i = 0, maxTiles = _maxXTiles * _maxZTiles; i < maxTiles; i++)
        {
            _worldTiles[i] = (TileType)bytes[i];
        }
    }

    private void Start()
    {
        _worldScaleX = worldFloor.transform.localScale.x;
        // whyy
        _worldScaleZ = worldFloor.transform.localScale.y;
        
        _maxXTiles = Mathf.FloorToInt(_worldScaleX / baseTilePrefab.transform.localScale.x);
        _maxZTiles = Mathf.FloorToInt(_worldScaleZ / baseTilePrefab.transform.localScale.z);
        
        Debug.Log($"Max tiles: {_maxXTiles}x{_maxZTiles}");

        _tileXSize = baseTilePrefab.transform.localScale.x;
        _tileZSize = baseTilePrefab.transform.localScale.z;
        
        Debug.Log($"Tile size: {_tileXSize}x{_tileZSize}");

        _startingXLocation =
            worldFloor.transform.localPosition.x + (_worldScaleX - _tileXSize) / 2;
        _startingZLocation =
            worldFloor.transform.localPosition.z + (_worldScaleZ - _tileZSize) / 2;
        
        Debug.Log($"Starting locations at {_startingXLocation}, {_startingZLocation}");

        _worldTiles = new TileType[_maxXTiles * _maxZTiles];
        Debug.Log($"Max amount of tiles is {_worldTiles.Length}");
}

    private void SpawnInTiles()
    {
        Debug.Log("Spawning tiles");
        for (int x = 0; x < _maxXTiles; x++)
        {
            for (int z = 0; z < _maxZTiles; z++)
            {
                Vector3 location = new Vector3(_startingXLocation - x * _tileXSize, 0, _startingZLocation - z * _tileZSize);
                Debug.Log($"Spawning tile at {location}");
                GameObject tileToSpawn = GetTile(_worldTiles[(x + 1) * z]);
                Instantiate(tileToSpawn, location, Quaternion.identity);
            }
        }
    }

    private GameObject GetTile(TileType tileType)
    {
        switch (tileType)
        {
            default:
                return baseTilePrefab;
        }
    }
}
