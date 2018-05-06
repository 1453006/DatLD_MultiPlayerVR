using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
public class Quest
{
    public QuestData questData { get; set; }
    public int process;
   
    public Quest(QuestData data)
    {
        this.questData = data;
        this.process = 0;
    }
}

public class QuestManager : Photon.MonoBehaviour{

    private List<Quest> listQuest = new List<Quest>();
    public Dictionary<int, ObjectInGame> dicQuestObject;
    public static QuestManager instance;
    public PopupQuestBoard questBoard;
    public GameObject gameProbs;

    /// <summary>
    /// time
    /// </summary>
    public double timeStart = -1;
    private float countDownDuration = 1800;
    private int displayTime = 0;
    public GroupGames_NetManager netmanager;
    private void Awake()
    {
        instance = this;
        netmanager.mineOnJoinedRoom += mineOnJoinedRoom;
    }
    // Use this for initialization
    void Start () {
        LoadListQuest();
        InitQuestObject();
       
        questBoard.UpdateListQuest(listQuest);
    }

    void mineOnJoinedRoom()
    {
        InitRoomProps();
        //Log props for test
        LogRoomProperties();
    }

    /// <summary>
    /// turn all object to their correct state
    /// </summary>
    public void SyncObjectWithRoomProbs()
    {
        if (PhotonNetwork.room == null)
            return;
        Quest q = null;
        for(int i =0;i< listQuest.Count;i++)
        {
            q = listQuest[i];
            string key = q.questData.mainQuestId + "/" + q.questData.subQuestId + "/" + q.questData.requireNum;
            object _process = PhotonNetwork.room.CustomProperties[key];
            if (_process == null || (int)_process == q.process)
                continue;
          
            q.process = (int) _process;

            //update GUI
            questBoard.UpdateSubQuestText(q);

            //update Object
            UpdateTutorialObject(q.questData.mainQuestId, q.questData.subQuestId, (int)_process);

            if ((int)_process >= q.questData.requireNum)
                SendQuestObjectChangeStage(i);
        }
    }

   public void LogRoomProperties()
    {
        foreach (Quest q in listQuest)
        {
            
           string key = q.questData.mainQuestId+"/"+q.questData.subQuestId+"/"+q.questData.requireNum;
           object val =  PhotonNetwork.room.CustomProperties[key];

            Debug.Log("<color=red>"+q.questData.mainQuestName + " -> "+val+"</color>");
        }
    }

    // Update is called once per frame
    void Update () {
        if (PhotonNetwork.room != null && PhotonNetwork.room.PlayerCount >= 2 && timeStart == -1 )
        { 
            if(PhotonNetwork.isMasterClient)
            {
                timeStart = PhotonNetwork.time;
                photonView.RPC("SendTimeStart",PhotonTargets.Others,timeStart);
                //save to room probs
                ExitGames.Client.Photon.Hashtable startTime = new ExitGames.Client.Photon.Hashtable() {
                { "TimeStart",timeStart } };
                PhotonNetwork.room.SetCustomProperties(startTime);
                
            }
        }

        if (timeStart != -1)
        {
            double timetick = PhotonNetwork.time - timeStart;
            int tmp = Mathf.RoundToInt((float)(countDownDuration - timetick));
            if (displayTime != tmp)
            {
                questBoard.UpdateCountDown(displayTime);
                displayTime = tmp;
            }
            if (displayTime <= 0)
            {
                /// Game Over
            }
        }
    }

    public void UpdateQuestProcess(QuestData qData , int num)
    {
        for (int i = 0; i < listQuest.Count; i++)
        {
            if (listQuest[i].questData.mainQuestId == qData.mainQuestId &&
               listQuest[i].questData.subQuestId == qData.subQuestId)
            {
                Quest q = listQuest[i];
                if (q.process < qData.requireNum)
                {
                    //q.process += num;
                    photonView.RPC("SendIncreaseQuestNum", PhotonTargets.AllViaServer,
                        i, num);
                }
                else
                {
                    //OnFinishQuest(q);
                    photonView.RPC("SendQuestObjectChangeStage", PhotonTargets.AllViaServer,
                       i);
                }
            }
        }
       
    }

    public void OnFinishQuest(Quest q)
    {
        //update Board GUI
        ObjectInGame go = null;
        dicQuestObject.TryGetValue(q.questData.mainQuestId, out go);
        if(go)
        {
            go.OnFinishQuest(q.questData.mainQuestId, q.questData.subQuestId);
        }
    }

    public bool isFinishQuest(QuestData qData)
    {
        Quest q = listQuest.Find(x => x.questData == qData);
        if (q != null)
        {
            if (q.process >= qData.requireNum)
                return true;
        }
        return false;
    }

    void LoadListQuest()
    {
        List<QuestData> constantQuest = GroupGameDatabase.instance.getAllConstantQuest();

        foreach(QuestData q in constantQuest)
        {
            Quest newQuest = new Quest(q);
            listQuest.Add(newQuest);
            Debug.Log(q.mainQuestName + " " + q.subQuestName);
        }
    }

    void InitQuestObject()
    {
        dicQuestObject = new Dictionary<int, ObjectInGame>();
        foreach(Quest q in  listQuest)
        {
            string goName = q.questData.questObjectName;
            Transform trf = null;
            trf = gameProbs.transform.findChildRecursively(goName); 
            if(trf)
            {
                ObjectInGame cond = null;
                dicQuestObject.TryGetValue(q.questData.mainQuestId,out cond);
                if (cond != null)
                    continue;

                GameObject go = trf.gameObject;
                ObjectInGame goCtrl  = go.addMissingComponent<ObjectInGame>();
                goCtrl.type = ObjectInGame.TYPE.QuestObject;
                goCtrl.extentData = q.questData.mainQuestId.ToString();

                go.addMissingComponent<BoxCollider>();

                dicQuestObject.Add(q.questData.mainQuestId, goCtrl);

                InitTutorialQuest(trf, q.questData.mainQuestId);
            }
        }

    }

    void InitTutorialQuest( Transform go,int mainQuestId)
    {
        GameObject tutor = FBPoolManager.instance.getPoolObject("Group_tutorial", FBPoolManager.POOLTYPE.UI);
        if(tutor)
        {
            tutor.transform.parent = go;
            tutor.transform.localPosition = Vector3.zero;
            
            PopupTutorial scrp = tutor.GetComponent<PopupTutorial>();
            scrp.mainQuestId = mainQuestId;
            scrp.belongTo = go.gameObject;
            scrp.ShowPopup();

            

        }
    }

    void InitRoomProps()
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        foreach(Quest q in listQuest)
        {
            //datld changed for test 0 ->  4
            ExitGames.Client.Photon.Hashtable questCheck = new ExitGames.Client.Photon.Hashtable() {
                { q.questData.mainQuestId+"/"+q.questData.subQuestId+"/"+q.questData.requireNum, (int)0 } };
            PhotonNetwork.room.SetCustomProperties(questCheck);
        }
    }

    void UpdateQuestProps(Quest q, int value)
    {
        string hashString = q.questData.mainQuestId + "/" + q.questData.subQuestId + "/" + q.questData.requireNum;
        int newprocess = (int)PhotonNetwork.room.CustomProperties[hashString];
        newprocess = value;

        ExitGames.Client.Photon.Hashtable questCheck = new ExitGames.Client.Photon.Hashtable() {
                {hashString, newprocess } };
        PhotonNetwork.room.SetCustomProperties(questCheck);

    }

    #region RPC
    [PunRPC]
    void SendIncreaseQuestNum(int questIndex, int num)
    {
        Quest q = listQuest[questIndex];
        if (q != null)
        {
            int val = q.process + num;
            if (val > q.questData.requireNum)
                return;
            else
            {
                q.process = val;

                //update GUI
                questBoard.UpdateSubQuestText(q);

                //update Object
                UpdateTutorialObject(q.questData.mainQuestId, q.questData.subQuestId, val);

                //update room properties
                if (PhotonNetwork.isMasterClient)
                    UpdateQuestProps(q, val);

                if (val >= q.questData.requireNum)
                    SendQuestObjectChangeStage(questIndex);
            }

        }
    }

    [PunRPC]
    void SendQuestObjectChangeStage(int questIndex)
    {
        Quest q = listQuest[questIndex];
        if (q != null)
        {
            int mainQuestId = q.questData.mainQuestId;
            ObjectInGame obj = null;
            dicQuestObject.TryGetValue(mainQuestId, out obj);
            if (!obj)
                return;
            if(obj.name.Contains("Prepare"))
                obj.OnFinishQuest(q.questData.mainQuestId,q.questData.subQuestId,true);
            else
                obj.OnFinishQuest(q.questData.mainQuestId, q.questData.subQuestId);

        }
    }

    void UpdateTutorialObject(int mainQuestId, int subQuestId, int num)
    {
        ObjectInGame obj = null;
        dicQuestObject.TryGetValue(mainQuestId, out obj);
        if (!obj)
            return;

        obj.GetComponentInChildren<PopupTutorial>().UpdateQuestProgress(mainQuestId, subQuestId, num);

    }

    [PunRPC]
    void SendStartGame(long timeStamp)
    {
        this.timeStart = timeStamp;

    }
    #endregion



}
