using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Panel : UdonSharpBehaviour
{
    [SerializeField] private TMP_Text resourcesDescription;
    [SerializeField] private TMP_Text currentOwnerDescription;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GatherableResourceData gatherableResourceData;
    [SerializeField] private GameObject resourcesTab;
    [SerializeField] private GameObject shopTab;
    [SerializeField] private GameObject donationTab;
    
    private PlayerWorkerUnit _playerWorker;
    private bool _showShopTab;

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        UpdateOwnership();
    }

    // not sure how to trigger an update when panel is activated so using this for now
    public void UpdateResourcesDescription()
    {
        resourcesDescription.text = playerInventory.GetInventoryContentDescription();
    }

    public void SetGatherLocationWood()
    {
        _playerWorker.SetGatherTarget(gatherableResourceData.GetResourceLocation(GatherableResourceType.Wood));
    }

    public void SetGatherLocationStone()
    {
        _playerWorker.SetGatherTarget(gatherableResourceData.GetResourceLocation(GatherableResourceType.Stone));
    }

    public void Donate10Wood()
    {
        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        playerInventory.DonateItems(GatherableResourceType.Wood, 10, owner);
    }

    public void Donate10Stone()
    {
        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        playerInventory.DonateItems(GatherableResourceType.Stone, 10, owner);
    }

    public void SwitchTab()
    {
        _showShopTab = !_showShopTab;
        UpdateActiveTab();
    }

    private void UpdateOwnership()
    {
        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        string local = owner.isLocal ? "(YOU!)" : "";
        VRCPlayerApi inventoryOwner = Networking.GetOwner(playerInventory.gameObject);
        string inventoryLocal = inventoryOwner.isLocal ? "YES" : "NO";
        currentOwnerDescription.text = $"Current Owner: {owner.displayName}{local} Inventory Owner: {inventoryOwner.displayName} Inventory Local: {inventoryLocal}";
        donationTab.gameObject.SetActive(!owner.isLocal);
    }

    private void UpdateActiveTab()
    {
        resourcesTab.SetActive(!_showShopTab);
        shopTab.SetActive(_showShopTab);
    }
    
    private void Start()
    {
        UpdateResourcesDescription();
        UpdateOwnership();
        GameObject[] playerObjects = Networking.GetPlayerObjects(Networking.LocalPlayer);
        foreach (GameObject playerObject in playerObjects)
        {
            _playerWorker = playerObject.GetComponent<PlayerWorkerUnit>();
            if (_playerWorker != null) break;
        }
        UpdateActiveTab();
    }
}
