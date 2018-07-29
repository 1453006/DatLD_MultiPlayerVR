using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon;
using ExitGames.Client.Photon;

public class GroupGames_NetManager: Photon.PunBehaviour {

    public GameObject avatarPrefabs;
    public GameObject[] listGroupObject;
    public GameObject weaponDock;
    public GameObject[] listWeapon;
    public const string setting = "1.0";

    public GameObject inventoryUI;

    public delegate void JoinedRoom();
    public event JoinedRoom mineOnJoinedRoom;

    public long startTime;

    private bool isJoinedRoom = false;
    private BasePopup textPopup = null;

    private void Awake()
    {

        GameObject obj = GameObject.Find("DontDestroyObject");
#if TEST_MAP
        if (obj)
            obj.SetActive(true);
#else
    if (obj)
            obj.SetActive(false);
#endif

    }

    // Use this for initialization
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(setting);
        var temp = PhotonVoiceNetwork.Client;

        FBSoundManager.PlayMusic("BackgroundMusic2");

        //GameObject fx =FBParticleManager.GetEffect("FX_Fire", 2f);
        //if (fx)
        //{
        //    fx.transform.position = Vector3.zero;
        //    fx.SetActive(true);
        //}
    }

    // Update is called once per frame
    void Update()
    {
       
        if(GvrControllerInput.AppButtonDown)
        {
            Player.instance.teleportController.OnTeleportClear();
            if (PopupManager.IsValidShowPopup())
            {
                PopupManager.ShowDialog(inventoryUI.name, -1);
            }
            else
            {
                //if is not valid close current popup
                PopupManager.DisableCurrentDialog();
            }

        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinOrCreateRoom("room_test", new RoomOptions() { IsVisible = true, MaxPlayers = 2 }, null);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
        foreach (RoomInfo game in PhotonNetwork.GetRoomList())
        {
            Debug.Log(game.name);
            Debug.Log(game.PlayerCount);
            Debug.Log(game.MaxPlayers);
        }

        PhotonNetwork.JoinOrCreateRoom("room_test", new RoomOptions() { IsVisible = true, MaxPlayers = 2 }, null);
    }

    //public virtual void OnPhotonRandomJoinFailed()
    //{
    //    Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
    //    PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 }, null);
    //}

    // the following methods are implemented to give you some context. re-implement them as needed.

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError("Cause: " + cause);
    }

    public override void OnJoinedRoom()
    {
        isJoinedRoom = true;

        if (Inventory.instance != null)
            Inventory.instance.LoadInventory();
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");

        Debug.Log(PhotonNetwork.room.Name);

        if(mineOnJoinedRoom !=null)
            mineOnJoinedRoom.Invoke();

        PhotonNetwork.Instantiate(avatarPrefabs.name, GamePlay.instance.spawnPoint, Quaternion.identity, 0);

        if (PhotonNetwork.isMasterClient)
        {
            foreach(GameObject obj in listGroupObject)
            {
                PhotonNetwork.Instantiate(obj.name, new Vector3(1,0,1), Quaternion.identity, 0);
            }
            
           for(int i =0;i< 4;i++)
            {
                if (listWeapon.Length <= i || listWeapon[i] == null)
                    continue;
                Vector3 pos = weaponDock.transform.findChildRecursively(i.ToString()).transform.position;
                pos.y += 0.2f;
                PhotonNetwork.Instantiate("GroupGame/"+ listWeapon[i].name, pos, Quaternion.identity, 0);
            }
        }
    }

   
    public override void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnPhotonCustomRoomPropertiesChanged(propertiesThatChanged);
        QuestManager.instance.LogRoomProperties();
        QuestManager.instance.SyncObjectWithRoomProbs();
    }

    private void OnApplicationFocus(bool focus)
    {
        //if not focus save game
        if(isJoinedRoom)
            Inventory.instance.SaveInventory();
    }


    public override void OnDisconnectedFromPhoton()
    {
        OnReturnLobby();
        Debug.Log("Disconnected from Photon");

    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log("PhotonPlayer left the room : " + otherPlayer.UserId);
        if (otherPlayer.IsMasterClient)
            OnReturnLobby();
    }

    private void OnReturnLobby()
    {
        textPopup = PopupManager.ShowText("No internet!!! return to lobby", 3);
        textPopup.eventShowPopupCompleted += OnShowPopupCompletted;

    }

    public void OnShowPopupCompletted()
    {
        textPopup.eventShowPopupCompleted -= OnShowPopupCompletted;
        FBFade.instance.fadeIn(2f);
        FBPoolManager.instance.returnAllObjectsToPool();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LobbyRoom", LoadSceneMode.Single);
    }


}
