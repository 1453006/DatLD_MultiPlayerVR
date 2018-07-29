using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour {

    private const string TEXT_POPUP = "TEXT";
    private const string YESNO_POPUP = "YESNO";
    private const string WRONG_SIGNAL_POPUP = "Group_WrongSignal";

    private static Vector3 playerPos;
    private static bool showedPopup;
    private static GameObject currentPopup;

    private static bool disableWhenMove = true;
    // Use this for initialization
    void Start () {
        showedPopup = false;
        playerPos = Vector3.zero;
    }
	
	// Update is called once per frame
	void Update () {
		if(showedPopup)
        {
            if (disableWhenMove && currentPopup && Player.instance.transform.position != playerPos)
            {
                DisableDialog(currentPopup);
            }
        }
	}

    public static void ShowText(string text, Vector3 position, float duration,bool isFollow = true)
    {
       GameObject go =  FBPoolManager.instance.getPoolObject(TEXT_POPUP,FBPoolManager.POOLTYPE.UI);
       BasePopup baseScript =  go.GetComponent<BasePopup>();
        baseScript.SetValues(null, isFollow, duration);
        baseScript.SetText(text);
        go.transform.position =  position;
        go.SetActive(true);
        currentPopup = go;
        showedPopup = true;
    }

    public static void ShowText(string text, GameObject parent, float duration, bool isFollow = true)
    {
        GameObject go = FBPoolManager.instance.getPoolObject(TEXT_POPUP, FBPoolManager.POOLTYPE.UI);
        BasePopup baseScript = go.GetComponent<BasePopup>();
        baseScript.SetValues(parent, isFollow, duration);
        baseScript.SetText(text);
        go.SetActive(true);
        currentPopup = go;
        showedPopup = true;
    }

    public static BasePopup ShowText(string text, float duration, bool isFollow = true)
    {
        GameObject go = FBPoolManager.instance.getPoolObject(TEXT_POPUP, FBPoolManager.POOLTYPE.UI);
        BasePopup baseScript = go.GetComponent<BasePopup>();
        baseScript.SetValues(null, isFollow, duration);
        baseScript.SetText(text);
        Vector3 targetPos = Camera.main.transform.position + Camera.main.transform.forward * 3f;
        targetPos.y = Player.instance.playerHeight + 1f;
        go.transform.position = targetPos;
        go.SetActive(true);
        currentPopup = go;
        playerPos = Player.instance.gameObject.transform.position;
        showedPopup = true;

        return baseScript;
    }

    public static GameObject ShowDialog(string dialogName, float duration, bool isFollow = true,bool _disableWhenMove = true)
    {
        
        GameObject go = FBPoolManager.instance.getPoolObject(dialogName, FBPoolManager.POOLTYPE.UI);
        BasePopup baseScript = go.GetComponent<BasePopup>();
        baseScript.SetValues(null, isFollow, duration);
        Vector3 targetPos = Camera.main.transform.position + Camera.main.transform.forward * 5f;
        targetPos.y = Player.instance.playerHeight + 0.1f;
        go.transform.position = targetPos;
        go.SetActive(true);
        currentPopup = go;
        disableWhenMove = _disableWhenMove;
        playerPos = Player.instance.gameObject.transform.position;
        showedPopup = true;
        return go;
    }

    public static GameObject ShowDialogBelongObject(string dialogName, float duration, Transform obj,  string markerName, bool isFollow = true, bool _disableWhenMove = true)
    {
        GameObject go = FBPoolManager.instance.getPoolObject(dialogName, FBPoolManager.POOLTYPE.UI);
        BasePopup baseScript = go.GetComponent<BasePopup>();
        baseScript.SetValues(null, isFollow, duration);
        Transform parentMarker = obj.transform.findChildRecursively(markerName);
        go.transform.position = parentMarker.transform.position;
        go.SetActive(true);
        currentPopup = go;
        disableWhenMove = _disableWhenMove;
        playerPos = Player.instance.gameObject.transform.position;
        showedPopup = true;
        return go;
    }

    public static GameObject ShowTimer(string dialogName, float duration, GameObject parentObj, bool isFollow = true)
    {
        GameObject go = FBPoolManager.instance.getPoolObject(dialogName, FBPoolManager.POOLTYPE.UI);
        BasePopup baseScript = go.GetComponent<BasePopup>();
        baseScript.SetValues(parentObj, isFollow, duration);
        go.SetActive(true);
       // currentPopup = go;
        disableWhenMove = false; //we dont want timer disable when player move  out
        playerPos = Player.instance.gameObject.transform.position;
      //  showedPopup = true;
        return go;
    }

    public static void DisableDialog(GameObject dialog)
    {
        if (!dialog)
            return;
       FBPoolManager.instance.returnObjectToPool(dialog,FBPoolManager.POOLTYPE.UI);
        if (Player.instance.currentState != Player.PlayerState.None)
            Player.instance.SetState(Player.PlayerState.None);
        showedPopup = false;
        currentPopup = null;
    }

   
    public static void DisableCurrentDialog()
    {

        DisableDialog(currentPopup);
    }

    public static bool IsValidShowPopup()
    {
        if(!showedPopup && !currentPopup)
        {
            return true;
        }
        return false;
    }

    public static void ShowWrongSignal(Vector3 pos,float duration = 1f, bool isFollow = true)
    {
        //GameObject go = FBPoolManager.instance.getPoolObject(WRONG_SIGNAL_POPUP, FBPoolManager.POOLTYPE.UI);
        //BasePopup baseScript = go.GetComponent<BasePopup>();
        //baseScript.SetValues(null, isFollow, duration);
        FBSoundManager.Play(FBSoundManager.WRONG_SOUND);
        GameObject go = FBParticleManager.GetEffect("wrong_signal", 2f);

        go.transform.position = pos;
        go.SetActive(true);
    }
}
