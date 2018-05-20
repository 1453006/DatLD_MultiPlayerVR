using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #region UI
    public GameObject quitDialog;

   

    #endregion
    private void Awake()
    {
        instance= this;
    }
    public void SetGame(Game game)
    {
        switch (game)
        {
            case Game.HOCKEY:
                {

                    break;
                }
            case Game.MATH:
                {
                    break;
                }
        }
    }
        // Use this for initialization
    void Start () {
        this.currentGame = Game.NONE;
	}

	
	// Update is called once per frame
	void Update () {
		
        if(GvrController.AppButtonDown)
        {
            if (PhotonNetwork.isMasterClient && currentGamePrefab != null)
                quitDialog = PopupManager.ShowDialogBelongObject(quitDialog.name, -1, currentGamePrefab.transform, FBTextManager.QUIT_GAME_MARKER);
        }
	}


    public void OnEndGame()
    {
        currentGamePrefab.GetComponent<GameCore>().OnDisableGame();
        PhotonNetwork.Destroy(currentGamePrefab);
        Player.instance.SetState(Player.PlayerState.None);
    }
}

