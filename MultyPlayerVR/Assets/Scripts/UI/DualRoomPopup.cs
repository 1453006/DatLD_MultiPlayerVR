using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualRoomPopup : MonoBehaviour {

    #region QuitDialog
    public void QuitDialog_YesBtnDown()
    {
       MiniGameManager.instance.OnEndGame();
        PopupManager.DisableDialog(this.gameObject);
    }

    public void QuitDialog_NoBtnDown()
    {
        PopupManager.DisableDialog(this.gameObject);
    }
    #endregion


}
