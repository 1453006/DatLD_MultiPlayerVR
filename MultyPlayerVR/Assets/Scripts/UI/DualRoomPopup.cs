using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualRoomPopup : MonoBehaviour {

    #region QuitDialog
    public void QuitDialog_YesBtnDown()
    {
        PopupManager.DisableDialog(this.gameObject);
    }

    public void QuitDialog_NoBtnDown()
    {
        PopupManager.DisableDialog(this.gameObject);
    }
    #endregion


    #region GoToLobby
    public void GoToLobby_YesBtnDown()
    {
        PopupManager.DisableDialog(this.gameObject);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("LobbyRoom");
    }

    public void GoToLobby_NoBtnDown()
    {
        PopupManager.DisableDialog(this.gameObject);
    }
    #endregion

    public void JoinRoom_YesBtnDown(string  type)
    {
        
    }


}
