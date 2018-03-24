using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GroupGames_NetManager : MonoBehaviour {

    public GameObject avatarPrefabs;
    public GameObject[] listGroupObject;
    public GameObject[] listWeapon;
    public const string setting = "1.0";

    public GameObject inventoryUI;
    // Use this for initialization
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(setting);
        var temp = PhotonVoiceNetwork.Client;
    }

    // Update is called once per frame
    void Update()
    {
        if(GvrControllerInput.AppButtonDown)
        {
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

    public virtual void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinOrCreateRoom("room_test", new RoomOptions() { IsVisible = true, MaxPlayers = 2 }, null);
    }

    public virtual void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinOrCreateRoom("room_test", new RoomOptions() { IsVisible = true, MaxPlayers = 2 }, null);
    }

    //public virtual void OnPhotonRandomJoinFailed()
    //{
    //    Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
    //    PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 }, null);
    //}

    // the following methods are implemented to give you some context. re-implement them as needed.

    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError("Cause: " + cause);
    }

    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");

        PhotonNetwork.Instantiate(avatarPrefabs.name, GamePlay.instance.spawnPoint, Quaternion.identity, 0);

        if (PhotonNetwork.isMasterClient)
        {
            foreach(GameObject obj in listGroupObject)
            {
                PhotonNetwork.Instantiate(obj.name, new Vector3(1,0,1), Quaternion.identity, 0);
            }

            foreach (GameObject obj in listWeapon)
            {
                PhotonNetwork.Instantiate(obj.name, new Vector3(Random.Range(-1f,1f), 0.5f, Random.Range(-1f, 1f)), Quaternion.identity, 0);
            }
        }
    }
}
