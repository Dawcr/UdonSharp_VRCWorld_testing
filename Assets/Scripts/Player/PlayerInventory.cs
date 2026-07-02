using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Persistence;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayerInventory : UdonSharpBehaviour
{
    // TODO: find a way better way to trigger panel update
    [SerializeField] private Panel panel;

    private VRCTweenHandle _autoSaveOperation;
    private const float AutoSaveTimer = 10f;
    // could be a struct (string name, int amount) but Udon# hates my attempts
    private readonly int[] _gatheredResources = new int[(int)GatherableResourceType.LENGTH];
    private int _experience;
    private const string ExperienceDataName = "TotalExperience";
    private const int ExperiencePerLevel = 100;
    
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
        _gatheredResources[(int)resource] += amount;
        _experience += amount;
        Debug.Log($"Added {amount} {GatherableResourceTypeExtensions.GetName(resource)} to inventory");
        panel.UpdateResourcesDescription();
    }

    public void RemoveItems(GatherableResourceType resource, int amount)
    {
        // stop autosave to avoid resource duplication shenanigans
        StopAutoSave();
        _gatheredResources[(int)resource] -= amount;
        Debug.Log($"Removed {amount} {GatherableResourceTypeExtensions.GetName(resource)} from inventory");
        panel.UpdateResourcesDescription();
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
