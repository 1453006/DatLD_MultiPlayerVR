using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class FBUtils
{
    /// <summary>
    /// swap 2 objects in a list
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    /// <param name="list">list</param>
    /// <param name="idx1">idx of object 1</param>
    /// <param name="idx2">idx of object 2</param>
    public static void swapObject<T>(List<T> list, int idx1, int idx2)
    {
        T obj1 = list[idx1];
        list[idx1] = list[idx2];
        list[idx2] = obj1;
    }

    /// <summary>
    /// add a component of type T and return it; if component exists, return it
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    /// <param name="obj">game object</param>
    /// <returns>added component</returns>
    public static T addMissingComponent<T>(this GameObject obj) where T : Component
    {
        T t = obj.GetComponent<T>();
        if (t == null)
            t = obj.AddComponent<T>();
        return t;
    }

    /// <summary>
    /// recursively find a child with specified name
    /// </summary>
    /// <param name="parent">parent</param>
    /// <param name="name">name to find</param>
    /// <returns>child</returns>
    public static Transform findChildRecursively(this Transform parent, string name)
    {
        Transform result = parent.Find(name);
        if (result)
            return result;
        foreach (Transform child in parent)
        {
            result = findChildRecursively(child, name);
            if (result)
                return result;
        }
        return null;
    }

    public static void DeleteAllChild(this Transform parent)
    {
        foreach(Transform child in  parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    public static void faceToMainCamera(this Transform parent)
    {
        parent.transform.LookAt(Camera.main.transform);
        parent.transform.rotation = Quaternion.Euler(0, parent.transform.rotation.eulerAngles.y-180, 0);
    }

    /// <summary>
    /// set layer for a game object and its children
    /// </summary>
    /// <param name="obj">game object</param>
    /// <param name="layer">layer id</param>
    public static void setLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            t.gameObject.setLayerRecursively(layer);
    }

    /// <summary>
    /// detach and destroy all children
    /// </summary>
    /// <param name="parent">parent</param>
    public static void destroyChildren(this Transform parent)
    {
        List<GameObject> list = new List<GameObject>(parent.childCount);
        for (int i = 0; i < parent.childCount; i++)
            list.Add(parent.GetChild(i).gameObject);
        parent.DetachChildren();
        for (int i = 0; i < list.Count; i++)
            GameObject.Destroy(list[i]);
    }

    public static void removeComponents<T>(this GameObject obj, bool removeInChildren = false)
    {
        T[] components = null;
        if (removeInChildren)
            components = obj.GetComponentsInChildren<T>();
        else
            components = obj.GetComponents<T>();
        for (int i = 0; i < components.Length; i++)
            GameObject.Destroy(components[i] as UnityEngine.Object);
    }

    public static int RandomEnumValue<T>()
    {
        return UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(T)).Length);
    
    }

    public static void DoAnimJumpOut(GameObject obj)
    {
        obj.addMissingComponent<BoxCollider>();
        Transform highlight = obj.transform.findChildRecursively(FBTextManager.HIGHLIGHT_WEAPON_PREF);

        if (!highlight)
        {
            highlight = FBPoolManager.instance.getPoolObject(FBTextManager.HIGHLIGHT_WEAPON_PREF).transform;
            if (highlight)
            {
                highlight.transform.SetParent(obj.transform);
                highlight.transform.localPosition = Vector3.zero;
            }
        }
        else
            highlight.gameObject.SetActive(false);

        float x = obj.transform.position.x + UnityEngine.Random.Range(-5f, 5f);
        float z = obj.transform.position.z + UnityEngine.Random.Range(-5f,5f);
        float y = GetGroundYAxis(new Vector3(x, 100f, z));
        obj.transform.DOJump(new Vector3(x, y, z), 4f, 1, 1f).OnComplete(() =>
        {
            //turn on highlight item
            if(highlight)
            {
                highlight.gameObject.SetActive(true);
            }
        });
        //obj.transform.localPosition = new Vector3(x, y, z);

    }

    public static void DoAnimScaleUp(GameObject obj)
    {
        
        Vector3 targetScale =  obj.transform.localScale;
        obj.transform.localScale /= 4f;
        obj.SetActive(true);
        obj.transform.DOScale(targetScale, 1f);

    }


    public static float GetGroundYAxis(Vector3 fakePos)
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = new Ray(fakePos, Vector3.down);
        if (Physics.Raycast(ray, out hit, 200f/*, 1<< LayerMask.NameToLayer("Ground")*/))
        {
            Debug.DrawRay(fakePos,hit.point, Color.green);
            return hit.point.y;
        }
       
        return -1;
    }
    
    public static void SaveListToFile<T>(T data,string filename)
    {
        string destination = Application.persistentDataPath + filename;
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);

       
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public static T ParseListFromFile<T>(string filename)
    {
        string destination = Application.persistentDataPath + filename;
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            Debug.LogError("File not found");
            return default(T);
        }

        BinaryFormatter bf = new BinaryFormatter();

        T result = (T)bf.Deserialize(file);
        file.Close();

        return result;
    }
    #region need review

    public static string Url;
    public static string videoUrl;
    public static string playVideo_previousScene;

    public static void playVideo()
    {
        videoUrl = Url;
        playVideo_previousScene = SceneManager.GetActiveScene().name;
        Application.LoadLevel("Video");
    }

    public static bool isValidUrl(string url)
    {
        string myurl = url;
        try
        {
            if (url.Contains("http://") || url.Contains("https://"))
                return true;
            return false;
        }
        catch (NullReferenceException ex)
        {

        }
        return false;
    }

    #endregion

#region simple
    public static bool PointInOABB(Vector3 point, BoxCollider box)
    {
        point = box.transform.InverseTransformPoint(point) - box.center;

        float halfX = (box.size.x * 0.5f);
        float halfY = (box.size.y * 0.5f);
        float halfZ = (box.size.z * 0.5f);
        if (point.x < halfX && point.x > -halfX &&
           point.y < halfY && point.y > -halfY &&
           point.z < halfZ && point.z > -halfZ)
            return true;
        else
            return false;
    }


    #endregion


}
