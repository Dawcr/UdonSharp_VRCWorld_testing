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
    [SerializeField] private GameObject resourcesTab;
    [SerializeField] private GameObject shopTab;
    [SerializeField] private GameObject[] activeOnlyForOwner;
    [SerializeField] private GameObject[] inactiveOnlyForOwner;
    
    private PlayerWorkerUnit _playerWorker;
    private bool _showShopTab;

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        HandleOwnership();
    }

    // not sure how to trigger an update when panel is activated so using this for now
    public void UpdateResourcesDescription()
    {
        resourcesDescription.text = playerInventory.GetInventoryContentDescription();
    }

    public void SetGatherLocationWood()
    {
        _playerWorker.GoGather(GatherableResourceType.Wood);
    }

    public void SetGatherLocationStone()
    {
        _playerWorker.GoGather(GatherableResourceType.Stone);
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
    
    private void Start()
    {
        HandleNullValues();
        UpdateResourcesDescription();
        HandleOwnership();
        RetrievePlayerWorker();
        UpdateActiveTab();
    }

    private void HandleOwnership()
    {
        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        UpdateDebugDescription(owner);
        SetObjectsActiveForLocalPlayer(owner.isLocal);
    }

    private void SetObjectsActiveForLocalPlayer(bool isLocal)
    {
        foreach (GameObject obj in activeOnlyForOwner)
        {
            obj.SetActive(isLocal);
        }

        foreach (GameObject obj in inactiveOnlyForOwner)
        {
            obj.SetActive(!isLocal);
        }
    }

    private void UpdateDebugDescription(VRCPlayerApi owner)
    {
        string local = owner.isLocal ? "(YOU!)" : "(Not you)";
        VRCPlayerApi inventoryOwner = Networking.GetOwner(playerInventory.gameObject);
        string inventoryLocal = inventoryOwner.isLocal ? "(YOU!)" : "(Not you)";
        currentOwnerDescription.text = $"Current Owner: {owner.displayName}{local}\nInventory Owner: {inventoryOwner.displayName}{inventoryLocal}";
    }

    private void UpdateActiveTab()
    {
        resourcesTab.SetActive(!_showShopTab);
        shopTab.SetActive(_showShopTab);
    }

    private void RetrievePlayerWorker()
    {
        GameObject[] playerObjects = Networking.GetPlayerObjects(Networking.LocalPlayer);
        foreach (GameObject playerObject in playerObjects)
        {
            _playerWorker = playerObject.GetComponent<PlayerWorkerUnit>();
            if (_playerWorker != null) break;
        }
    }

    private void HandleNullValues()
    {
        if (resourcesDescription == null)
        {
            Debug.LogError("ResourcesDescription in Panel script is null");
        }

        if (currentOwnerDescription == null)
        {
            Debug.LogError("CurrentOwnerDescription in Panel script is null");
        }

        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory in Panel script is null");
        }

        if (resourcesTab == null)
        {
            Debug.LogError("ResourcesTab in Panel script is null");
        }
        
        if  (shopTab == null)
        {
            Debug.LogError("ShopTab in Panel script is null");
        }
    }
}
