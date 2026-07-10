using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Persistence;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerInventory : UdonSharpBehaviour
{
    // TODO: find a way better way to trigger panel update
    [SerializeField] private Panel panel;

    private VRCTweenHandle _autoSaveOperation;
    private const float AutoSaveTimer = 60f;
    // could be a struct (string name, int amount) but Udon# hates my attempts
    private readonly int[] _gatheredResources = new int[(int)GatherableResourceType.LENGTH];
    private int _experience;
    private const string ExperienceDataName = "TotalExperience";
    private const int ExperiencePerLevel = 100;
    private const int ExperiencePerDonation = 2;
    
    public string GetInventoryContentDescription()
    {
        StringBuilder builder = new StringBuilder();
        for (var i = 0; i < _gatheredResources.Length; i++)
        {
            builder.AppendLine($"{GatherableResourceTypeExtensions.GetName(i)} amount: {_gatheredResources[i]}");
        }
        builder.AppendLine($"Level: {_experience / ExperiencePerLevel} ({_experience % ExperiencePerLevel}/{ExperiencePerLevel})");
        return builder.ToString();
    }

    public void AddItems(GatherableResourceType resource, int amount)
    {
        _experience += amount;
        AddItemsToInventory(resource, amount);
    }

    public void AddItemsToInventory(GatherableResourceType resource, int amount)
    {
        _gatheredResources[(int)resource] += amount;
        Debug.Log($"Added {amount} {GatherableResourceTypeExtensions.GetName(resource)} to inventory");
        panel.UpdateResourcesDescription();
    }

    public bool TryRemoveItems(GatherableResourceType resource, int amount, int bonusExperienceIfSuccessful)
    {
        if (_gatheredResources[(int)resource] < amount) return false;
        // stop autosave to avoid resource duplication shenanigans
        StopAutoSave();
        _gatheredResources[(int)resource] -= amount;
        Debug.Log($"Removed {amount} {GatherableResourceTypeExtensions.GetName(resource)} from inventory");
        _experience += bonusExperienceIfSuccessful;
        panel.UpdateResourcesDescription();
        Save();
        ScheduleNextAutoSave();
        return true;
    }

    public bool TryToPay(int[] resources)
    {
        for (int i = 0; i < resources.Length; i++)
        {
            if (_gatheredResources[i] < resources[i]) return false;
        }

        for (int i = 0; i < resources.Length; i++)
        {
            TryRemoveItems((GatherableResourceType)i, resources[i], 0);
        }
        
        return true;
    }

    public void DonateItems(GatherableResourceType resource, int amount, VRCPlayerApi targetPlayer)
    {
        int expFromDonation = amount * ExperiencePerDonation;
        if (TryRemoveItems(resource, amount, expFromDonation))
        {
            string name = targetPlayer.displayName;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Others, nameof(ReceiveDonation),
                resource, amount, name);
        }
    }

    [NetworkCallable]
    public void ReceiveDonation(GatherableResourceType resource, int amount, string targetPlayerName)
    {
        if (Networking.LocalPlayer.displayName != targetPlayerName) return; // is this really the best way?
        StopAutoSave();
        AddItemsToInventory(resource, amount);
        Save();
        ScheduleNextAutoSave();
    }

    public void AutoSave()
    {
        Save();
        ScheduleNextAutoSave();
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        if (!player.isLocal) return;
        
        for (int i = 0; i < _gatheredResources.Length; i++)
        {
            _gatheredResources[i] = PlayerData.GetInt(Networking.LocalPlayer, GatherableResourceTypeExtensions.GetName(i));
        }
        _experience = PlayerData.GetInt(Networking.LocalPlayer, ExperienceDataName);
        panel.UpdateResourcesDescription();
        ScheduleNextAutoSave();
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (!Networking.IsOwner(other.gameObject)) return;
        
        PlayerWorkerUnit worker = other.GetComponent<PlayerWorkerUnit>();
        if (worker == null) return;
        
        worker.DropResource();
    }

    private void Save()
    {
        Debug.Log("Saving");
        for (int i = 0; i < _gatheredResources.Length; i++)
        {
            PlayerData.SetInt(GatherableResourceTypeExtensions.GetName(i), _gatheredResources[i]);
        }
        PlayerData.SetInt(ExperienceDataName, _experience);
    }

    private void ScheduleNextAutoSave()
    {
        StopAutoSave();
        _autoSaveOperation = VRCTween.DelayedCall(this, nameof(AutoSave), AutoSaveTimer);
    }

    private void StopAutoSave()
    {
        if (_autoSaveOperation.IsActive)
        {
            _autoSaveOperation.Kill();
        }
    }
}
