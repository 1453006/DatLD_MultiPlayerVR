using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemData
{
    public Item Item { get; set; }
    public int amount { get; set; }
    public int SlotIndex { get; set; }
}

public class CombinationMap
{
    public int ItemId01 { get; set; }
    public int ItemId02 { get; set; }
    public int ItemIdFinal { get; set; }
    public float Duration { get; set; }
}


public class ItemDatabase : MonoBehaviour {

    private List<Item> itemDatabase = new List<Item>();
    public static ItemDatabase instance;

    private List<CombinationMap> combinationDatabase = new List<CombinationMap>();

    
    private void Awake()
    {
        instance = this;
        FBGameData.instance.loadGameDatabase("Data");
        ConstructItemDatabase();
        ConstructCombinationMapDatabase();
    }

    public  Item getItemData(string name)
    {
        name = name.Replace("(Clone)", "");
        name = name.Trim();
        return itemDatabase.Find(x => x.prefabName == name);
    }

 

    void Start()
    {
      

        foreach(Item item in itemDatabase)
        {
            Debug.Log("item: " + item.Id + " " + item.Title + " " + item.Description);
        }
    }

    public Item GetItemByID(int id)
    {
        for (int i = 0; i < itemDatabase.Count; i++)
        {
            if (itemDatabase[i].Id == id)
            {
                return itemDatabase[i];
            }
        }

        return null;
    }

    void ConstructItemDatabase()
    {
        Dictionary<int,FBClassObject> listAllItems = FBGameData.instance.getClassData("GroupGameItems").objects;
        foreach(KeyValuePair<int,FBClassObject> item in listAllItems)
        {
            FBClassObject itemData = item.Value;
            Item newItem = new Item();
            newItem.Id = (int)itemData.getFieldValue("itemId").intValue;
            newItem.Title = itemData.getFieldValue("title").stringValue;
            newItem.Power = (int)itemData.getFieldValue("power").floatValue;
            newItem.Description = itemData.getFieldValue("description").stringValue;
            newItem.iconName = itemData.getFieldValue("iconName").stringValue;
            newItem.prefabName = itemData.getFieldValue("prefabName").stringValue;
            newItem.Sprite = Resources.Load<Sprite>("Prefabs/UI/Sprites/Items/" + newItem.iconName);
            newItem.Stackable = itemData.getFieldValue("stackable").intValue;
            newItem.ReceiveItemId = itemData.getFieldValue("receiveItemId").intValue;
            //newItem.Prefabs = FBPoolManager.instance.getPoolObject(newItem.prefabName);

            itemDatabase.Add(newItem);
        }
    }

    #region Combination map
    void ConstructCombinationMapDatabase()
    {
        Dictionary<int, FBClassObject> listAllItems = FBGameData.instance.getClassData("CombinationMap").objects;
        foreach (KeyValuePair<int, FBClassObject> item in listAllItems)
        {
            FBClassObject itemData = item.Value;
            CombinationMap newMap = new CombinationMap();
            newMap.ItemId01 = itemData.getFieldValue("itemId01").intValue;
            newMap.ItemId02 = itemData.getFieldValue("itemId02").intValue;
            newMap.ItemIdFinal = itemData.getFieldValue("itemidFinal").intValue;
            newMap.Duration = itemData.getFieldValue("duration").floatValue;
            //newItem.Prefabs = FBPoolManager.instance.getPoolObject(newItem.prefabName);

            combinationDatabase.Add(newMap);
        }
    }

    public int getOtherCombineObjId(int objId)
    {
        int result = -1;
        foreach(CombinationMap map in combinationDatabase)
        {
            if (map.ItemId01 == objId)
                return map.ItemId02;
            else if (map.ItemId02 == objId)
                return map.ItemId01;
        }
        return result;
    }

    public CombinationMap getCombinationMapById(int objectId)
    { 
        foreach (CombinationMap map in combinationDatabase)
        {
            if (map.ItemId01 == objectId)
                return map;
            else if (map.ItemId02 == objectId)
                return map;
        }
        return null;
    }
    #endregion
}
