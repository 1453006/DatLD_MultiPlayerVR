using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData
{
    public Item Item;
    public int itemId;
    public int amount;
    public int SlotIndex;
}

[System.Serializable]
public class ItemSaveData
{
    public int itemId;
    public int amount;
}

[System.Serializable]
public class ListItemSaveData
{
    public List<ItemSaveData> list;
}

[System.Serializable]
public class listItemData
{
    public ItemData[] list;
}

public class CombinationMap
{
    public int ItemId01 { get; set; }
    public int ItemId02 { get; set; }
    public int ItemIdFinal { get; set; }
    public float Duration { get; set; }
}

public class QuestData
{
    public int mainQuestId { get; set; }
    public string mainQuestName { get; set; }
    public int subQuestId { get; set; }
    public string subQuestName { get; set; }
    public int requireItemId { get; set; }
    public int requireNum { get; set; }
    public string questObjectName { get; set; }
    public string questObjectRewardPref { get; set; }
}



public class GroupGameDatabase : MonoBehaviour {

    private List<Item> itemDatabase = new List<Item>();
    public static GroupGameDatabase instance;

    private List<CombinationMap> combinationDatabase = new List<CombinationMap>();
    private List<QuestData> listQuestData = new List<QuestData>();
    
    private void Awake()
    {
        instance = this;
        FBGameData.instance.loadGameDatabase("Data");
        ConstructItemDatabase();
        ConstructCombinationMapDatabase();
        ConstructQuestDatabase();
    }

    public  Item getItemData(string name)
    {
        string[] split = name.Split(' ');
        split[0] = split[0].Replace("(Clone)", "");
        return itemDatabase.Find(x => x.prefabName == split[0].Trim());
    }

    public Item getItemData(int itemId)
    {
        return itemDatabase.Find(x => x.Id == itemId);
    }

    public Sprite GetItemIcon(int itemId)
    {
        Item item = itemDatabase.Find(x => x.Id == itemId);
        if (item.Id != -1)
            return item.Sprite;
        return null;
    }

    public string GetItemPrefabName(int itemId)
    {
        Item item = itemDatabase.Find(x => x.Id == itemId);
        if (item.Id != -1)
            return item.prefabName;
        return null;
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

    #region QuestData
    void ConstructQuestDatabase()
    {
        Dictionary<int, FBClassObject> listAllQuest = FBGameData.instance.getClassData("GroupGameQuest").objects;
        foreach (KeyValuePair<int, FBClassObject> item in listAllQuest)
        {
            FBClassObject itemData = item.Value;
            QuestData q = new QuestData();
            q.mainQuestId = item.Value.getFieldValue("mainQuestId").intValue;
            q.mainQuestName = item.Value.getFieldValue("mainQuestName").stringValue;
            q.subQuestId = item.Value.getFieldValue("subQuestId").intValue;
            q.subQuestName = item.Value.getFieldValue("subQuestName").stringValue;
            q.requireItemId = item.Value.getFieldValue("requireItemId").intValue;
            q.requireNum = item.Value.getFieldValue("requireNum").intValue;
            q.questObjectName = item.Value.getFieldValue("questObjectName").stringValue;
            q.questObjectRewardPref = item.Value.getFieldValue("questObjectRewardPref").stringValue;
            listQuestData.Add(q);
        }
    }

    public List<QuestData> getAllConstantQuest()
    {
        return listQuestData;
    }

    public List<QuestData> getAllQuestViaMainId(int mainQuestId)
    {
        List<QuestData> result = new List<QuestData>();
        foreach (var item in listQuestData)
        {
            if (item.mainQuestId == mainQuestId)
                result.Add(item);
        }
        return result;
    }
    #endregion
}
