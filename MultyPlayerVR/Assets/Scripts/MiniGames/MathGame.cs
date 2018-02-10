using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MathGame : GameCore, IPunTurnManagerCallbacks
{

    public static MathGame instance;
    public GameObject balloon;
    public GameObject gun ;
    public Transform spawnArea;
    private BoxCollider boxSpawnArea;

    #region CONSTANT
    private const float MIN_DIST_BETWEEN_BALLOONS = 3.0f;
    #endregion

    private List<GameObject> listObjInGame;
    public enum Operator {
        ADD = 0,
        SUBSTRACT,
        MULTI
    };

    public enum ResultType
    {
       
        LocalWin = 0,
        LocalLoss
    }

    private PunTurnManager turnManager;
    short countDownDuration = 5;
    // keep track of when we show the results to handle game logic.
    private bool IsShowingResults;

    /// <summary>
    ///  GAME VARIABLES
    /// </summary>
    public int numberA, numberB;
    public Operator op;
    private int localAnswer, correctNumber;
    private ResultType result;

#region UI
    public Text masterQuest;
    public Text remoteQuest;
    public Text[] masterAnswer;
    public Text[] remoteAnswer;
#endregion

    private void Awake()
    {
        instance = this;
       
    }

    // Use this for initialization
    void Start () {
        this.turnManager = this.gameObject.AddComponent<PunTurnManager>();
        this.turnManager.TurnManagerListener = this;
        this.turnManager.TurnDuration = 5f;
        listObjInGame = new List<GameObject>();
        boxSpawnArea = spawnArea.GetComponent<BoxCollider>();
    }
	
	// Update is called once per frame
	void Update () {

        UpdateGameCore();
        int displayTime = 10000;
        base.OnUpdateGUI();
        if (currentState == State.CountDown && base.startTime != 0)
        {
            double timetick = PhotonNetwork.time - base.startTime;
            displayTime = Mathf.RoundToInt((float)(countDownDuration - timetick));
            countDown.text = displayTime.ToString();
            if (displayTime <= 0)
            {
                base.SetState(State.Start);
                //countDown.gameObject.SetActive(false);
            }
        }

        if (this.turnManager.IsOver)
        {
            return;
        }

        /*
		// check if we ran out of time, in which case we loose
		if (turnEnd<0f && !IsShowingResults)
		{
				Debug.Log("Calling OnTurnCompleted with turnEnd ="+turnEnd);
				OnTurnCompleted(-1);
				return;
		}
	*/
       
        if (this.turnManager.Turn > 0)
        {
            this.UpdatePlayerScore();
            if (!IsShowingResults)
            this.countDown.text = this.turnManager.RemainingSecondsInTurn.ToString("F1") + " SECONDS";
        }

      

    }

    public override void OnStartGame()
    {
        base.OnStartGame();
        //spawn gun
        int parentId =  Player.instance.visualPlayer.GetPhotonView().viewID;
        photonView.RPC("SpawnObjOverNetwork", PhotonTargets.AllViaServer, gun.name, parentId);

        if (this.turnManager.Turn == 0)
        {
            // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
            GenerateNewQuestion();
            photonView.RPC("SendQuestion", PhotonTargets.AllViaServer, this.numberA, this.numberB, this.op);
            this.StartTurn();
        }

    }

    #region RPC
    [PunRPC]
    public void SpawnObjOverNetwork(string objName,int parentViewId)
    {
       PhotonView view = PhotonView.Find(parentViewId);
        if (view)
        {
            GameObject gunObj = PhotonNetwork.Instantiate(objName, Vector3.zero, Quaternion.identity, 0);
            Transform rightHand = view.gameObject.transform.findChildRecursively("Hand_Right_jnt");
            gunObj.transform.SetParent(rightHand);
            gunObj.transform.localPosition = Vector3.zero;
            gunObj.transform.localRotation = Quaternion.identity;
        }
    }
    #endregion

    #region TurnManager Callbacks

    /// <summary>Called when a turn begins (Master Client set a new Turn number).</summary>
    public void OnTurnBegins(int turn)
    {
        Debug.Log("OnTurnBegins() turn: " + turn);
        IsShowingResults = false;
        if(PhotonNetwork.isMasterClient)
        {
            GenerateNewQuestion();
        }
     
    }


    public void OnTurnTimeEnds(int obj)
    {
        if (!IsShowingResults)
        {
            Debug.Log("OnTurnTimeEnds: Calling OnTurnCompleted");
            OnTurnCompleted(-1);
        }
    }


    public void OnTurnCompleted(int obj)
    {
        Debug.Log("OnTurnCompleted: " + obj);
        this.CalculateWinAndLoss();
        this.UpdateScores();
        this.OnEndTurn();
    }


    // when a player moved (but did not finish the turn)
    public void OnPlayerMove(PhotonPlayer photonPlayer, int turn, object move)
    {
        Debug.Log("OnPlayerMove: " + photonPlayer + " turn: " + turn + " action: " + move);
       
    }


    // when a player made the last/final move in a turn
    public void OnPlayerFinished(PhotonPlayer photonPlayer, int turn, object move)
    {
        Debug.Log("OnTurnFinished: " + photonPlayer + " turn: " + turn + " action: " + move);

        if (photonPlayer.IsLocal)
        {
            this.localAnswer = (int)(byte)move;
        }

    }


    private void UpdateScores()
    {
        if (this.result == ResultType.LocalWin)
        {
            PhotonNetwork.player.AddScore(1);   // this is an extension method for PhotonPlayer. you can see it's implementation
        }

    }

    #endregion


    #region Core Gameplay Methods


    /// <summary>Call to start the turn (only the Master Client will send this).</summary>
    public void StartTurn()
    {
        if (PhotonNetwork.isMasterClient)
        { 
            this.turnManager.BeginTurn();
        }
    }

    public void MakeTurn(int answer)
    {
        this.turnManager.SendMove((byte)answer, true);
    }

    public bool isSentMove()
    {
       return  this.turnManager.IsFinishedByMe;
    }

    public void OnEndTurn()
    {
        this.StartCoroutine("ShowResultsBeginNextTurnCoroutine");
    }

    public IEnumerator ShowResultsBeginNextTurnCoroutine()
    {

        IsShowingResults = true;
        GenerateNewQuestion();
        if(PhotonNetwork.isMasterClient)
            photonView.RPC("SendQuestion", PhotonTargets.AllViaServer, this.numberA, this.numberB, this.op);
        yield return new WaitForSeconds(2.0f);

        //DATLD
        this.StartTurn();
    }


    public void EndGame()
    {
        Debug.Log("EndGame");
    }

    private void CalculateWinAndLoss()
    {
        Debug.Log("calc " + localAnswer + ":" + CalcExpression(this.numberA, this.numberB, this.op));
        if (localAnswer == correctNumber)
        {
            this.result = ResultType.LocalWin;
            Debug.Log(" YOU WIN");
        }
        else
        {
            this.result = ResultType.LocalLoss;
            Debug.Log("YOU LOSS");
        }

    }

    void GenerateNewQuestion()
    {
        this.numberA = Mathf.RoundToInt(Random.Range(0, 100));
        this.numberB = Mathf.RoundToInt(Random.Range(2, 100));
        this.op = (Operator)Mathf.RoundToInt(Random.Range(0, 2));
    }

    void UpdateGameUI()
    {
        char displayeOp = new char();
        switch(this.op)
        {
            case Operator.ADD:
                displayeOp = '+';
                break;
            case Operator.MULTI:
                displayeOp = 'x';
                break;
            case Operator.SUBSTRACT:
                displayeOp = '-';
                break;

        }

        masterQuest.text = this.numberA + " " + displayeOp + " " + this.numberB + "= ";

        //if (PhotonNetwork.isMasterClient)
        //{
        //    masterQuest.text = this.numberA + " " + displayeOp + " " + this.numberB + "= ";
        //    int correctAnswerPos = Mathf.RoundToInt(Random.Range(0, 2));
        //    correctNumber = CalcExpression(this.numberA, this.numberB, this.op);
        //    masterAnswer[correctAnswerPos].text = correctNumber.ToString();

        //    for (int i = 0; i < masterAnswer.Length; i++)
        //    {
        //        if (i == correctAnswerPos)
        //            continue;
        //        else
        //        {
        //            masterAnswer[i].text = CalcExpression(correctNumber, Random.RandomRange(10, 20), (Operator)Mathf.RoundToInt(Random.Range(0, 2))).ToString();
        //        }
        //    }
        //}
        //else
        //{
        //    //is remote
        //    remoteQuest.text = this.numberA + " " + displayeOp + " " + this.numberB + "= ";
        //    int correctAnswerPos = Mathf.RoundToInt(Random.Range(0, 2));
        //    correctNumber = CalcExpression(this.numberA, this.numberB, this.op);
        //    remoteAnswer[correctAnswerPos].text = correctNumber.ToString();

        //    for (int i = 0; i < remoteAnswer.Length; i++)
        //    {
        //        if (i == correctAnswerPos)
        //            continue;
        //        else
        //        {
        //            remoteAnswer[i].text = CalcExpression(correctNumber, Random.RandomRange(10, 20), (Operator)Mathf.RoundToInt(Random.Range(0, 2))).ToString();
        //        }
        //    }
        //}

    }
        
    [PunRPC]
    void SendQuestion(int numberA,int numberB ,Operator op)
    {
        this.numberA = numberA;
        this.numberB = numberB;
        this.op = op;
        if (PhotonNetwork.isMasterClient)
        {
            correctNumber = CalcExpression(numberA, numberB, op);
            //delete previous turn object
            foreach (GameObject go in listObjInGame)
                PhotonNetwork.Destroy(go);
            listObjInGame.Clear();

            //add new one
            int numSpawn = Random.Range(6,10);
            int[] correctAnswerPos = new int[2];
            for (int i = 0; i < correctAnswerPos.Length; i++)
            {
                correctAnswerPos[i] = Random.Range(0,numSpawn);
                //check with previous pos
                if (i > 0)
                {
                    while (correctAnswerPos[i] == correctAnswerPos[i - 1])
                    {
                        correctAnswerPos[i] = Random.Range(0, numSpawn);
                    }
                }
            }

            for (int i = 0; i < numSpawn; i++)
            {
                GameObject obj = null;
                if (i == correctAnswerPos[0] || i == correctAnswerPos[1])
                {
                    obj = SpawnQuest(correctNumber);
                }
                else
                {
                    int wrongNumber = CalcExpression(correctNumber, Random.RandomRange(10, 20), (Operator)Mathf.RoundToInt(Random.Range(0, 2)));
                    obj = SpawnQuest(wrongNumber);
                }
                listObjInGame.Add(obj);
            }
           
        }
            UpdateGameUI();
    }

    bool isTooClose(Vector3 pos)
    {
        foreach (var item in listObjInGame)
        {
            if (Vector3.Distance(item.transform.position, pos) < MIN_DIST_BETWEEN_BALLOONS)
                return true;
        }
        return false;
    }

    GameObject SpawnQuest(int correctNum)
    {
        Vector3 rndPosWithin;
        do
        {
            if (boxSpawnArea)
            {
                Vector3 sz = boxSpawnArea.size;
                rndPosWithin = new Vector3(Random.Range(-sz.x, sz.x), Random.Range(-sz.y, sz.y), Random.Range(-sz.z, sz.z));
            }
            else
                rndPosWithin = new Vector3(Random.Range(-6f, 6f), Random.Range(-3f, 3f), Random.Range(-5f, 5f));
            rndPosWithin = spawnArea.TransformPoint(rndPosWithin *.5f );
        }
        while (isTooClose(rndPosWithin));

        GameObject obj = PhotonNetwork.Instantiate(balloon.name, rndPosWithin, Quaternion.identity, 0);
        TextMesh txt = obj.GetComponentInChildren<TextMesh>();
        txt.text = correctNum.ToString();
        return obj;
    }

    int CalcExpression(int a, int b,Operator op)
    {
        int result = 0;

        switch (op)
        {
            case Operator.ADD:
                {
                    result = a + b;
                    break;
                }
            case Operator.SUBSTRACT:
                {
                    result = a - b;
                    break;
                }
            case Operator.MULTI:
                {
                    result = a * b;
                    break;
                }
        }

        return result;

    }

    void UpdatePlayerScore()
    {
        PhotonPlayer master = null;
        PhotonPlayer remote = null;

        if (PhotonNetwork.connected && PhotonNetwork.room.PlayerCount == 1)
        {
            master = PhotonNetwork.player;
            remote = null;
        }
        else
        {

            master = PhotonNetwork.isMasterClient ? PhotonNetwork.player : PhotonNetwork.player.GetNext();
            remote = master.GetNext();
        }

        base.txtScore_master.text = master.GetScore().ToString("D2");
        base.txtScore_remote.text = remote != null ? remote.GetScore().ToString("D2") : "0";


    }

    #endregion


}
