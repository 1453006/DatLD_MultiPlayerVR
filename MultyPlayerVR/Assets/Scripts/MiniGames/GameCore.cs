﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;

public class GameCore : PunBehaviour
{
    public enum ResultType
    {
        None = 0,
        Draw,
        LocalWin,
        LocalLoss
    }

    // note that master position always 0, others are remote position
    public Transform[] playerPos;
    public State currentState;
    public GameType currentGame;

    public List<GameObject> listInGameObj;

    public float countDownDelay = 1f;
    public double startTime = 0;

    bool isCalledRestart = false;

    #region UI
    public Text countDown;
    public Text Result;
    public Text txtScore_master, txtScore_remote;
    #endregion

    #region 2 Players
    public int score_master = 0;
    public int score_remote = 0;
#endregion

    public enum GameType
    {
        Hockey = 0

    }

    public enum State
    {
        None,
        Waiting ,
        CountDown,
        Start,
        Playing,
       
    };

    // Use this for initialization
    void Start()
    {
      
    }


    private void Awake()
    {
        SetState(State.None);
    }
    // Update is called once per frame
    void Update()
    {
       
    }
    public void OnUpdateGUI()
    {
       // countDown.transform.faceToMainCamera();
       // txtScore_master.transform.faceToMainCamera();
        //txtScore_remote.transform.faceToMainCamera();
    }
    public void OnSetGUI()
    {

        countDown.text = 0.ToString();
        txtScore_master.text = score_master.ToString();
        txtScore_remote.text = score_remote.ToString();
    }

    public virtual void OnDisableGame()
    {

        if (listInGameObj != null && listInGameObj.Count > 0)
        {
            foreach (GameObject go in listInGameObj)
                PhotonNetwork.Destroy(go);
        }
        this.photonView.RPC("ResetPlayerState", PhotonTargets.Others, Player.PlayerState.None);
    }
    void StartCountDown()
    {
        OnCountDown();
    }

    public void SetState(State state)
    {
        switch (state)
        {
            case State.Waiting:{
                    OnSetGUI();
                    break;
                }
	            case State.CountDown:{
	                   //Player.instance.ShowPlayerMsgViaPhoton("Starting game...",1f);
                      
                    Invoke("StartCountDown",countDownDelay);
	                    break;
                }
            case State.Start:
                {
                    OnStartGame();
                    break;
                }
            case State.Playing:
                {
                    break;
                }
          
        }
        currentState = state;
    }

    protected void UpdateGameCore()
    {
        if (currentState == State.None)
        {
            if (PhotonNetwork.room != null && PhotonNetwork.room.PlayerCount == 1)
            {
                Debug.Log("OnJoinedRoom: Player Count == 2");
                SetState(State.CountDown);
            }
            else
            {

            }
        }
    }


    public virtual void OnStartGame()
    {
        //score_master = score_remote = 0;


    }

  


    public virtual void OnCountDown()
    {
        //set player Position to be correct
        Player.instance.SetState(Player.PlayerState.PlayingGame);
        this.photonView.RPC("SetPlayerPosition", PhotonTargets.AllViaServer);
        if (PhotonNetwork.isMasterClient)
        {
            this.photonView.RPC("SetStartTime", PhotonTargets.AllViaServer, PhotonNetwork.time);
        }

    }
    #region PUN Behaviour
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.room.PlayerCount == FBTextManager.NUM_START_PLAY)
        {
            Debug.Log("OnJoinedRoom: Player Count == 2");
            SetState(State.CountDown);
        }
        else
        {
            Debug.Log("Waiting for another player");
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Other player arrived");

        if (PhotonNetwork.room.PlayerCount == FBTextManager.NUM_START_PLAY)
        {
            Debug.Log("OnPhotonPlayerConnected: Player Count == 2");
            SetState(State.CountDown);

        }
    }


    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log("Other player disconnected! " + otherPlayer.ToStringFull());
    }


    public override void OnConnectionFail(DisconnectCause cause)
    {

    }
    #endregion

#region RPC FUNCTION

    [PunRPC]
    public void SetPlayerPosition()
    {
      FBFade.instance.fadeIn(2f);
        if (PhotonNetwork.isMasterClient)
        {
            //Player.instance.MoveTo(playerPos[0].position, playerPos[0].rotation);
            Player.instance.SnapTo(playerPos[0].position, playerPos[0].rotation);
        }
        else
        {
            //Player.instance.MoveTo(playerPos[1].position, playerPos[1].rotation);
            Player.instance.SnapTo(playerPos[1].position, playerPos[1].rotation);
        }
    }

    [PunRPC]
    public void SetStartTime(double time)
    {
        startTime = time;
    }

    [PunRPC]
    public void AddScore2Players(int score, int index)
    {
        if (index == 0) //is master
        {
            score_master += 1;
            txtScore_master.text = score_master.ToString();
        }
        else
        {
            score_remote += 1;
            txtScore_remote.text = score_remote.ToString();
        }


    }
    [PunRPC]
    public void ResetPlayerState(Player.PlayerState state)
    {
        Player.instance.SetState(state);
        if (listInGameObj != null && listInGameObj.Count > 0)
        {
            foreach (GameObject go in listInGameObj)
                PhotonNetwork.Destroy(go);
        }
        PopupManager.ShowText("Ending game...",3f);
    }

    [PunRPC]
    public void OnRestartGame(double timestamp)
    {
        startTime = timestamp;
        countDownDelay = 0f;
        SetState(State.CountDown);
   
    }
    #endregion

}

