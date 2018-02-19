using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class for single item to be pool
[System.Serializable]
public class ObjectPoolItem
{
	/// <summary>
	/// index of free object in pool
	/// </summary>
	int freeIndex;

	[HideInInspector]
	public string name;

	public GameObject objectToPool;
	public int amountToPool = 2;
	public bool isExpandable = true;
	public List<GameObject> pooledObjects;

	public void init()
	{
		name = objectToPool.name;

		pooledObjects = new List<GameObject>();
		for (int i = 0; i < amountToPool; i++)
		{
			GameObject obj = GameObject.Instantiate<GameObject>(objectToPool, FBPoolManager.instance.transform);
			obj.name = name;
			obj.SetActive(false);
			pooledObjects.Add(obj);
		}

		freeIndex = 0;
	}

	public GameObject getPoolObject()
	{
		if (freeIndex < pooledObjects.Count)
			return pooledObjects[freeIndex++];

		// pool not available
		if (isExpandable)
		{
			// create new
			GameObject obj = GameObject.Instantiate<GameObject>(objectToPool, FBPoolManager.instance.transform);
			obj.name = name;
			obj.SetActive(false);
			pooledObjects.Add(obj);
			freeIndex++;
			return obj;
		}

		return null;
	}

	public bool returnObjectToPool(GameObject obj)
	{
		for (int i = 0; i < freeIndex; i++)
		{
			// swap returned object with object above freeIndex;
			if (pooledObjects[i] == obj)
			{
				obj.SetActive(false);
				obj.transform.parent = FBPoolManager.instance.transform;
				FBUtils.swapObject<GameObject>(pooledObjects, freeIndex - 1, i);
				--freeIndex;
				return true;
			}
		}

		// not found
		return false;
	}

	public void returnAllObjectsToPool()
	{
		for(int i=0; i<pooledObjects.Count; i++)
		{
			pooledObjects[i].SetActive(false);
			pooledObjects[i].transform.parent = FBPoolManager.instance.transform;
		}
		freeIndex = 0;
	}
}

public class FBPoolManager : MonoBehaviour
{
    public enum POOLTYPE
    {
        DEFAULT,
        UI
    }

	public static FBPoolManager instance;
	public List<ObjectPoolItem> itemsToPool;
    public List<ObjectPoolItem> uiToPool;
    Dictionary<string, ObjectPoolItem> dicItemsToPool = new Dictionary<string, ObjectPoolItem>();
    Dictionary<string, ObjectPoolItem> dicUIToPool = new Dictionary<string, ObjectPoolItem>();
    void Awake()
	{
		instance = this;

		foreach (ObjectPoolItem item in itemsToPool)
		{
			item.init();
			dicItemsToPool.Add(item.name, item);
		}

        foreach (ObjectPoolItem item in uiToPool)
        {
            item.init();
            dicUIToPool.Add(item.name, item);
        }

    }

	/// <summary>
	/// gets a pooled object with specified name
	/// </summary>
	/// <returns>pooled object</returns>
	/// <param name="name">name</param>
	public GameObject getPoolObject(string name, POOLTYPE type = POOLTYPE.DEFAULT)
	{
        switch (type)
        {
            case POOLTYPE.DEFAULT:
                return dicItemsToPool[name].getPoolObject();
               
            case POOLTYPE.UI:
                return dicUIToPool[name].getPoolObject();
              
            default:
                break;
        }

        return null;

    }

	/// <summary>
	/// returns an object to pool
	/// </summary>
	/// <param name="obj">object to return</param>
	/// <returns>true if obj belongs to the pool, false otherwise</returns>
	public bool returnObjectToPool(GameObject obj, POOLTYPE type = POOLTYPE.DEFAULT)
	{
		ObjectPoolItem objectPoolItem;
        if (type == POOLTYPE.DEFAULT)
        {
            if (dicItemsToPool.TryGetValue(obj.name, out objectPoolItem))
                return objectPoolItem.returnObjectToPool(obj);
        }
        else if (type == POOLTYPE.UI)
        {
            if (dicUIToPool.TryGetValue(obj.name, out objectPoolItem))
                return objectPoolItem.returnObjectToPool(obj);
        }
		return false;
	}

	/// <summary>
	/// returns all objects to pool
	/// </summary>
	public void returnAllObjectsToPool()
	{
		for (int i = 0; i < itemsToPool.Count; i++)
			itemsToPool[i].returnAllObjectsToPool();
	}
}
