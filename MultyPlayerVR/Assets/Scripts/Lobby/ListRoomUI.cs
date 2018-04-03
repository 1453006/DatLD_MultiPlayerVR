using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum UIType
{
    DUAL,
    GROUP
}
public class ListRoomUI : MonoBehaviour {

    public Transform containList;
    public Button RoomInfoBtn;
    public UIType type;
    
    
    public void OnDrawUI(List<RoomInfo> listRoom)
    {

        containList.DeleteAllChild();
        foreach(RoomInfo room in listRoom)
        {
            //create button
            GameObject btn = GameObject.Instantiate(RoomInfoBtn.gameObject, containList);
            btn.SetActive(true);
            btn.GetComponent<Button>().onClick.AddListener(delegate { OnSetCurrentRoom(room.Name); });
            btn.GetComponentInChildren<Text>().text = room.Name;

        }
       
    }

    void OnSetCurrentRoom(string roomName)
    {
        switch (type)
        {
            case UIType.DUAL:
                LobbyNetmanager.instance.currentDualRoom = roomName;
                break;
            case UIType.GROUP:
                LobbyNetmanager.instance.currentGroupRoom = roomName;
                break;
            default:
                break;
        }
    }

    
}
