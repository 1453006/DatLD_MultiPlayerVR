using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNetmanager : MonoBehaviour{

    public const string setting = "1.0";
    public GameObject avatarPrefabs;

    public List<RoomInfo> listDualData;
    public List<RoomInfo> listGroupData;

    public GameObject listDualUI;
    public GameObject listGroupUI;
    public GameObject dialogCreateNewRoom;

    public string currentGroupRoom { get; set; }
    public string currentDualRoom { get; set; }
    private int totalRoomCount = 0;

    public static LobbyNetmanager instance;

    private void Awake()
    {
        PhotonNetwork.autoJoinLobby = true;
        instance = this;
    }

   
    private bool isJoinedLobby = false;
    
    // Use this for initialization
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(setting);
        var temp = PhotonVoiceNetwork.Client;

        OnInitPlayerOffline();
    }

    // Update is called once per frame
    void Update () {
		if(isJoinedLobby)
        {
            if(PhotonNetwork.GetRoomList().Length != totalRoomCount)
            {
                OnRefreshListRoom();
            }
        }
	}

    public virtual void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
        listGroupData = new List<RoomInfo>();
        listDualData = new List<RoomInfo>();
        totalRoomCount = PhotonNetwork.GetRoomList().Length;
        OnRefreshListRoom();

        isJoinedLobby = true;
    }

    void OnRefreshListRoom()
    {
        listGroupData.Clear();
        listDualData.Clear();
        foreach (RoomInfo game in PhotonNetwork.GetRoomList())
        {
            if (game.Name.Contains("group"))
                listGroupData.Add(game);
            else if (game.Name.Contains("dual"))
                listDualData.Add(game);
        }
        
        //update dual list ui
        listDualUI.GetComponent<ListRoomUI>().OnDrawUI(listDualData);
        //update group list ui
        listGroupUI.GetComponent<ListRoomUI>().OnDrawUI(listGroupData);
    }
    void OnInitPlayerOffline()
    {
        GameObject playerAvt =FBPoolManager.instance.getPoolObject("Avatar_02");
        playerAvt.transform.position = GamePlay.instance.spawnPoint;
        playerAvt.transform.rotation = Quaternion.identity;
        playerAvt.GetComponent<NetworkPlayer>().ChangeToOffline();
        playerAvt.SetActive(true);

    }

    public void CreateOrJoinRoom(string type)
    {
       if(type == "group")
        {
            if (currentGroupRoom == null)
            {
                GameObject dialog = PopupManager.ShowDialog(dialogCreateNewRoom.name, -1);
                Transform btnOkay = dialog.transform.findChildRecursively("BtnYES");
                Transform btnNo = dialog.transform.findChildRecursively("BtnNO");
                if (btnOkay)
                {
                    btnOkay.GetComponent<Button>().onClick.AddListener(delegate { OnCreateNewRoom("group"); });
                }
                if(btnNo)
                {
                    btnNo.GetComponent<Button>().onClick.AddListener(delegate { OnDisableDialog(dialog); });
                }
                return;
            }
            PhotonNetwork.JoinOrCreateRoom(currentGroupRoom, new RoomOptions() { IsVisible = true, MaxPlayers = 4 }, null);
        }
       else if ( type == "dual")
        {
            if (currentDualRoom == null)
            {
                GameObject dialog = PopupManager.ShowDialog(dialogCreateNewRoom.name, -1);
                Transform btnOkay = dialog.transform.findChildRecursively("BtnYES");
                Transform btnNo = dialog.transform.findChildRecursively("BtnNO");
                if (btnOkay)
                {
                    btnOkay.GetComponent<Button>().onClick.AddListener(delegate { OnCreateNewRoom("dual"); });
                }
                if (btnNo)
                {
                    btnNo.GetComponent<Button>().onClick.AddListener(delegate { OnDisableDialog(dialog); });
                }
                return;
            }
            PhotonNetwork.JoinOrCreateRoom(currentDualRoom, new RoomOptions() { IsVisible = true, MaxPlayers = 2 }, null);
        }
    }

    void OnCreateNewRoom(string type)
    {
        string newRoomName;
        int index = 0;
        byte numPlayer = 0;
        if (type == "group")
        {
            index = listGroupData.Count + 1;
            numPlayer = 4;
        }
        else if (type == "dual")
        {
            index = listDualData.Count + 1;
            numPlayer = 2;
        }
        newRoomName = type + index;
        PopupManager.DisableCurrentDialog();
        if ( PhotonNetwork.CreateRoom(newRoomName, new RoomOptions() { IsVisible = true,IsOpen = true, MaxPlayers = numPlayer }, null))
        {
            Debug.Log("Send create new room successfully");
            PhotonNetwork.LoadLevel("GroupRoom");
        }

        
       


    }

    void OnDisableDialog(GameObject dialog)
    {
        PopupManager.DisableDialog(dialog);
    }

    void OnReceivedRoomListUpdate()
    {
        var roomsList = PhotonNetwork.GetRoomList().Length;
        if(totalRoomCount != roomsList)
        {
            OnRefreshListRoom();
        }
    }
}
