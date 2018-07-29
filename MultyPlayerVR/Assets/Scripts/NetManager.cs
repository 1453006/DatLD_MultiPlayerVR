using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NetManager :  Photon.PunBehaviour
{

    public GameObject avatarPrefabs;
    public GameObject interactiveObjectPrefabs;

    public const string setting = "1.0";

    private BasePopup textPopup = null;

    private void Awake()
    {
//#if UNITY_EDITOR
//        GameObject obj = GameObject.Find("DontDestroyObject");
//        if (obj)
//        {
//            obj.SetActive(false);
//        }
//#endif
    }
    // Use this for initialization
    void Start () {
        PhotonNetwork.ConnectUsingSettings(setting);
        var temp = PhotonVoiceNetwork.Client;
        //FBSoundManager.PlayMusic("dual_theme");

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinOrCreateRoom("lobby0", new RoomOptions() { MaxPlayers = 4 }, null);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinRandomRoom();
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 }, null);
    }

    // the following methods are implemented to give you some context. re-implement them as needed.

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
      //  Debug.LogError("Cause: " + cause);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
        PhotonNetwork.Instantiate(avatarPrefabs.name, GamePlay.instance.spawnPoint, Quaternion.identity, 0);
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
        textPopup= PopupManager.ShowText("No internet!!! return to lobby", 3);
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
