using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class OwnerTab : UdonSharpBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    // this smells error-prone hmm
    [SerializeField] private GameObject[] trackedObjects;
    
    private VRCPlayerApi[] _players;

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        dropdown.interactable = player.isLocal;
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i].playerId == player.playerId)
            {
                dropdown.value = i;
                break;
            }
        }
        RefreshOptions();
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        RefreshOptions();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        RefreshOptions();
    }

    public void ChangeOwner()
    {
        ChangeOwner(_players[dropdown.value]);
    }
    
    private void ChangeOwner(VRCPlayerApi player)
    {
        foreach (var obj in trackedObjects)
        {
            Networking.SetOwner(player, obj);
        }
    }
    
    private void Start()
    {
        HandleNullValues();
    }

    private void HandleNullValues()
    {
        if (dropdown == null)
        {
            Debug.LogError($"Dropdown in OwnerTab script is null");
        }
    }

    private void RefreshOptions()
    {
        dropdown.ClearOptions();
        _players = VRCPlayerApi.GetPlayers();
        string[] playerNames = new string[_players.Length];
        for (int i = 0; i < _players.Length; i++)
        {
            playerNames[i] = _players[i].displayName;
        }
        dropdown.AddOptions(playerNames);
    }
}
