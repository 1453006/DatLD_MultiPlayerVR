using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameManager : MonoBehaviour {
    public enum Game
    {
        NONE,
        HOCKEY,
        MATH,
    }

    public Game currentGame;
    public static MiniGameManager instance;
    public GameObject currentGamePrefab;

    [Header("Button switch games")]
    public GameObject BtnMath;
    public GameObject BtnHockey;

    #region UI
    public GameObject quitDialog;
    public bool isShowingQuit = false;

   

    #endregion
    private void Awake()
    {
        instance= this;
    }
    
        // Use this for initialization
    void Start () {
        this.currentGame = Game.NONE;
	}

	
	// Update is called once per frame
	void Update () {
		
        if(GvrController.AppButtonDown)
        {
            if (isShowingQuit)
                return;
            if (PhotonNetwork.isMasterClient && currentGamePrefab != null)
                quitDialog = PopupManager.ShowDialogBelongObject(quitDialog.name, -1, currentGamePrefab.transform, FBTextManager.QUIT_GAME_MARKER);
            isShowingQuit = true;
            Transform btnOkay = quitDialog.transform.findChildRecursively("BtnYES");
            Transform btnNo = quitDialog.transform.findChildRecursively("BtnNO");
           
            if (btnOkay)
            {
                btnOkay.GetComponent<Button>().onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
                btnOkay.GetComponent<Button>().onClick.AddListener(() => OnEndGame());
            }
            if (btnNo)
            {
                btnNo.GetComponent<Button>().onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.Off);
                btnNo.GetComponent<Button>().onClick.AddListener(() => { PopupManager.DisableDialog(quitDialog); isShowingQuit = false; });
            }
        }

       
	}


    public void OnEndGame()
    {
        currentGamePrefab.GetComponent<GameCore>().OnDisableGame();
        PhotonNetwork.Destroy(currentGamePrefab);
        Player.instance.SetState(Player.PlayerState.None);
        PopupManager.DisableDialog(quitDialog);
        BtnMath.SetActive(true);
        BtnHockey.SetActive(true);
        isShowingQuit = false;
    }
}

