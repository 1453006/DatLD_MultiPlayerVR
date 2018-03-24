using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InventoryType
{
    SLOT,
    ITEM
}
public class InventoryItem : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{

    //slot
    public int index { get; set; }
    public ItemData itemData { get; set; }
    private InventoryType type;

    InventoryItem instance;

    private void Awake()
    {
        instance = this;
    }

    public void InitData(int index, InventoryType type, ItemData itemData = null)
    {
        this.index = index;
        this.type = type;
        this.itemData = itemData;
    }

#region EventSystem
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("click on item");
        if(type == InventoryType.ITEM &&  Player.instance.networkPlayer)
        {
            if (!Inventory.instance.RemoveItem(itemData.Item.Id, 1))
                return;
            //add back current hand item if had
            if(Player.instance.currentHandItem)
            {
                Item olddata = Player.instance.currentHandItem.GetComponent<Item>();
                if (olddata.Id != -1)
                    Inventory.instance.AddItem(olddata.Id);
            }

            PhotonView playerPhotonView = Player.instance.visualPlayer.GetPhotonView();
            playerPhotonView.RPC("SendAttachItemToHand", PhotonTargets.AllViaServer, itemData.Item.prefabName , playerPhotonView.viewID);
            //Player.instance.SetState(Player.PlayerState.None);
        }
    }
    public void OnPointerEnter(PointerEventData data)
    {
        if (type == InventoryType.ITEM)
        {
            Debug.Log(itemData.Item.Description);
        }
        Player.instance.SetState(Player.PlayerState.Selecting);
    }
    public void OnPointerExit(PointerEventData data)
    {
        Debug.Log("exit");
        Player.instance.SetState(Player.PlayerState.None);

    }
    #endregion
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
