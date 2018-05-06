using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

    public GameObject inventorySlot;
    public GameObject inventoryItem;

    public List<ItemData> listData = new List<ItemData>();
    public List<GameObject> listSlot = new List<GameObject>();

    public static Inventory instance;
    public const int SLOT_MAX = 12;

    private void Awake()
    {
        instance = this;
    }

    private void OnDisable()
    {
        if(Player.instance != null)
            Player.instance.SetState(Player.PlayerState.None);
    }
    // Use this for initialization
    void Start () {
   
        Transform slotPanel = this.transform.findChildRecursively("SlotPanel");
        for (int i = 0; i < SLOT_MAX; i++)
        {
            GameObject slot = Instantiate(inventorySlot);
           
            slot.addMissingComponent<InventoryItem>().InitData(i, InventoryType.SLOT);
            listSlot.Add(slot);
            listSlot[i].GetComponent<Slot>().id = i;
            listSlot[i].transform.SetParent(slotPanel);
            slot.transform.localScale = new Vector3(1, 1, 1);
            slot.transform.localPosition = Vector3.zero;
        }

       
        AddItem(1,3);
        AddItem(2,5);
        AddItem(3,10);

        SaveInventory();


    }

    void SaveInventory()
    {
        string saveData = JsonHelper.arrayToJson<ItemData>(listData.ToArray());


        Debug.Log("<color=yellow> mydata  is" + saveData + " </color>");

        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void UpdateDisplay()
    {
        //remove all items
        foreach(GameObject slot in listSlot)
        {
            slot.transform.destroyChildren();
        }

        foreach (ItemData data in listData)
        {
            AddItem(data.Item.Id, data.amount);
        }
    }

    public void AddItem(int id, int amountToAdd = 1)
    {
        Item itemToAdd = GroupGameDatabase.instance.GetItemByID(id);
        if (itemToAdd.Stackable == 1 && CheckIfItemIsInInventory(itemToAdd))
        {
            for (int i = 0; i < listData.Count; i++)
            {
                if(listData[i].Item.Id == id)
                {

                    listData[i].amount += amountToAdd;
                    //update amount text
                    UpdateAmountDisplay(i, listData[i].amount);
                }
            }
        }
        else
        {
            foreach (var slot in listSlot)
            { 
                if (slot.transform.childCount == 0)
                {
                    GameObject itemObj = Instantiate(inventoryItem);
                    ItemData itemData = new ItemData { Item = itemToAdd,itemId = itemToAdd.Id, amount = amountToAdd, SlotIndex = slot.GetComponent<InventoryItem>().index };
                    listData.Add(itemData);
                    InventoryItem scrp = itemObj.addMissingComponent<InventoryItem>();
                    scrp.InitData(-1, InventoryType.ITEM, itemData);
                    itemObj.transform.SetParent(slot.transform);
                    itemObj.transform.localScale = new Vector3(1, 1, 1);
                    itemObj.transform.localPosition = Vector3.zero;
                    itemObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    itemObj.GetComponent<Image>().sprite = itemToAdd.Sprite;
                    itemObj.name = "Item: " + itemToAdd.Title;
                    slot.name = itemToAdd.Id.ToString().Trim();

                    //update display
                    slot.transform.findChildRecursively("StackAmount").GetComponent<Text>().text = amountToAdd.ToString();
                    break;
                }
            }
        }
    }

    public bool RemoveItem(int id,int amountToRemove = 1)
    {
        Item itemToAdd = GroupGameDatabase.instance.GetItemByID(id);
        if (CheckIfItemIsInInventory(itemToAdd))
        {
            for (int i = 0; i < listData.Count; i++)
            {
                if (listData[i].Item.Id == id)
                {
                    if (listData[i].amount <= 0)
                    {
                        listData[i].amount = 0;
                        return false;
                    }
                     listData[i].amount -= amountToRemove;
                    UpdateAmountDisplay(i, listData[i].amount);
                    //update amount text
                    
                    break;
                }
            }
           


        }
        return true;
       
    }

    void UpdateAmountDisplay(int listDataIndex,int amount)
    {
        ItemData itemData = listData[listDataIndex];
        if(itemData != null)
        {
            int slotIndex = itemData.SlotIndex;
            foreach(GameObject slot in listSlot)
            {
                if(slot.GetComponent<Slot>().id == slotIndex)
                {
                    slot.transform.findChildRecursively("StackAmount").GetComponent<Text>().text = amount.ToString();
                    return;
                }
            }
        }
    }
    bool CheckIfItemIsInInventory(Item item)
    {
        bool cond1 = false;
        bool cond2 = false;
       
        //check if item exists in listdata
        for (int i = 0; i < listData.Count; i++)
        {
            if (listData[i].Item.Id == item.Id)
            {
                cond1 = true;
                break;
            }
        }

        //check if item exists in slot display
        foreach (GameObject slot in listSlot)
            if (slot.name == item.Id.ToString().Trim())
            {
                cond2 = (slot.transform.GetChildCount() > 0);
                break;
            }

        return cond1 && cond2;
    }

    public bool HasWeapon()
    {
        foreach (var item in listData)
        {
            if (item.Item.prefabName.Contains("Weapon"))
                return true;
        }
        return false;
    }
}
