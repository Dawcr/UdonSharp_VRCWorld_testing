using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TileChanger : UdonSharpBehaviour
{
    [SerializeField] private TileType tileToChangeTo;
    
    public TileType TargetTile => tileToChangeTo;
}
