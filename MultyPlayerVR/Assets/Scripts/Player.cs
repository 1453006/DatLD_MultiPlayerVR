﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour {

    public LoadCirclePointer loadcircle;
#region CONFIG
    public float playerHeight = 1.8f;
    #endregion

#region Player Util
    public void MoveTo(Vector3 pos,Quaternion rot)
    {
        Vector3 correctPos = new Vector3(pos.x, pos.y + playerHeight, pos.z);
        this.transform.DOMove(correctPos, 1f);
        
        this.transform.rotation = rot;
        if (visualPlayer)
        {
            visualPlayer.transform.DOMove(correctPos, 1f);
            visualPlayer.transform.rotation = rot;
        }

    }

    public void SnapTo(Vector3 pos, Quaternion rot)
    {
        Vector3 correctPos = new Vector3(pos.x, pos.y + playerHeight, pos.z);
        this.transform.position = correctPos;
        this.transform.rotation = rot;
        if (visualPlayer)
        {
            visualPlayer.transform.position = correctPos;
            visualPlayer.transform.rotation = rot;
        }

    }
    #endregion

    public GameObject defaultLaser;
    public GameObject visualPlayer;
    public NetworkPlayer networkPlayer;
    //This for swipe actions
    public GameObject swipeControllerGO;

    public Transform currentPointerGO;
 


    public DaydreamElements.Teleport.TeleportController teleportController;
    public static Player instance;
  
    public bool isHandAttached = false;
    public GameObject currentHandItem = null;
    public enum PlayerState
    {
        None,
        Teleporting,
        Selecting,
        PlayingGame,
        
    };

    private void Awake()
    {
        instance = this;
    }

    public GameObject getVisualRightHand()
    {
        if(visualPlayer)
        {
            NetworkPlayer netPlayer = visualPlayer.GetComponent<NetworkPlayer>();
            return netPlayer.visualHandTransform.gameObject;
        }
        return null;
    }

    public PlayerState currentState;

	// Use this for initialization
	void Start () {
        transform.position = GamePlay.instance.spawnPoint;
        if(swipeControllerGO)
            swipeControllerGO.GetComponent<SwipeController>().OnSwipeSelect += OnSwipeSelect;
    }

    #region SWIPE
    bool isSendSwipe = false;
    public bool isSendShoot = false;

    float shootDelay = 0f;
    const float DELAY_SHOOT = 0.5f;
    //swipe action callback
    private void OnSwipeSelect(SwipeAngle index)
    {
        switch (index)
        {
            case SwipeAngle.TOP:
                {
                    Debug.Log("Swipe up!!!");
                    if (isSendSwipe)
                        return;
                    OnSwipeUp();
                }
                break;
            case SwipeAngle.RIGHT:
                {
                    Debug.Log("Swipe RIGHT!!!");
                }
                break;
            case SwipeAngle.DOWN:
                {
                    Debug.Log("Swipe DOWN!!!");
                }
                break;
            case SwipeAngle.LEFT:
                {
                    Debug.Log("Swipe LEFT!!!");
                }
                break;
            default:
                break;
        }
    }

    void OnSwipeUp()
    {
        if(currentHandItem && !isSendShoot)
        {
            FBSoundManager.Play("BallThrow");

            //GameObject fx = FBParticleManager.GetEffect("FX_Fire", 2f);
            //if current hand item is gun
            ObjectInGame scrp = currentHandItem.GetComponent<ObjectInGame>();
            if(scrp && scrp.IsGunObjectInGame())
            {
                isSendShoot = true;
                OnSwipeUpForGun(scrp);
                
                return;
            }

            if (scrp && scrp.IsMeleetInGame())
                return;// not throw weapon

            isSendSwipe = true;
            currentHandItem.transform.SetParent(null);
            Vector3 endPos = teleportController.selectionResult.selection;
            if (currentPointerGO)
                endPos = currentPointerGO.position;
            Quaternion endRot = currentHandItem.transform.rotation;
            GameObject newItem = PhotonNetwork.Instantiate("GroupGame/"+currentHandItem.gameObject.name, endPos, endRot, 0);

            //test only delete it
            //fx.transform.position = endPos;
            //fx.SetActive(true);
            //test
            newItem.SetActive(false);
            //another player will see this player hand on nothing
            PhotonView playerPhotonView = visualPlayer.GetPhotonView();
            playerPhotonView.RPC("SendRemoveHandItem", PhotonTargets.Others,playerPhotonView.viewID);

            currentHandItem.transform.DOJump(endPos, 0.5f, 1, 1).OnComplete(() =>
           {
               isSendSwipe = false;
               FBPoolManager.instance.returnObjectToPool(currentHandItem);
               currentHandItem = null;
               newItem.SetActive(true);
              
           });
           
        }
       
    }

    void OnSwipeUpForGun(ObjectInGame objectScrp)
    {
        //swipe up to throw bullet from gun
        objectScrp.throwBullet();
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        //test voice
        UpdateState();
        if (visualPlayer)
        {
            visualPlayer.transform.position = this.transform.position;
            visualPlayer.transform.rotation = this.transform.rotation;
        }

        if (isSendShoot)
        {
            shootDelay += Time.deltaTime;
            if (shootDelay >= DELAY_SHOOT)
            {
                shootDelay = 0f;
                isSendShoot = false;
            }
        }

        if (isHandAttached)
        {
            if (GvrControllerInput.HomeButtonDown)
            {

            }
        }

        
    
    }

    public void SetState(PlayerState state)
    {
        switch (state)
        {
          
            case PlayerState.None:
                {
                    Invoke("enableTeleport", 1f);
                    defaultLaser.SetActive(true);
                }
                break;
            case PlayerState.Teleporting:
                {
                    defaultLaser.SetActive(false);
                    teleportController.gameObject.SetActive(true);
                }
                break;
            case PlayerState.Selecting:
                {
                    defaultLaser.SetActive(true);
                    //teleportController.gameObject.SetActive(false);
                }
                break;
            case PlayerState.PlayingGame:
                {
                    defaultLaser.SetActive(true);
                    //teleportController.gameObject.SetActive(false);

                }
                break;
            
            default:
                break;
        }

        currentState = state;

    }

    public void enableTeleport()
    {
        teleportController.gameObject.SetActive(true);
    }

    void UpdateState()
    {
        if (currentState == PlayerState.None)
        {

        }
        else if (currentState == PlayerState.Selecting)
        {
            teleportController.gameObject.SetActive(false);
        }
        else if (currentState == PlayerState.PlayingGame)
        {
            teleportController.gameObject.SetActive(false);
        }
       


    }

    public void OnBackToLobby()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("LobbyRoom");
    }

 }
