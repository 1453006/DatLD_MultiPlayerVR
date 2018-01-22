using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour {

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
    #endregion

    public GameObject defaultLaser;
    public GameObject visualPlayer;
    public DaydreamElements.Teleport.TeleportController teleportController;
    public static Player instance;
  
    private bool isHandAttached = false;
    public Transform handItem = null;
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
  
    public PlayerState currentState;

	// Use this for initialization
	void Start () {
        transform.position = GamePlay.instance.spawnPoint;
	}

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
        if (isHandAttached)
        {
            if(GvrControllerInput.HomeButtonDown)
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
                    teleportController.gameObject.SetActive(false);
                }
                break;
            case PlayerState.PlayingGame:
                {
                    defaultLaser.SetActive(true);
                    teleportController.gameObject.SetActive(false);

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

    
    public void OnAttachItemToHand(Transform item)
    {
        //SetState(PlayerState.Holding); 
        if (visualPlayer)
        {
            handItem = item;
            isHandAttached = true;
            item.GetComponent<Rigidbody>().isKinematic = true;
            NetworkPlayer netPlayer = visualPlayer.GetComponent<NetworkPlayer>();
            netPlayer.AttachHandItem(item);
            item.gameObject.layer = 2;
        }
    }

    public void OnDetachItemFromHand()
    {
        if (visualPlayer)
        {
            handItem.SetParent(null);
        }
    }

 }
