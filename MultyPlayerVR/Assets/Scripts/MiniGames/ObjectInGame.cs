using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectInGame : Photon.MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler{
    public string extentData;
    public GameObject extentGO;

    Rigidbody rigidBody;
    ObjectInGame instance;

    private Vector3 correctPos;
    private Quaternion correctRot;

    private MeshRenderer meshRenderer;

    private Vector3 lastPos;

    private Vector3 direct;
    private Vector3 correctDirect;
    public float speed;

    private float currentSpeed;
    private float correctSpeed;

    private float initScaleY;
    

    public enum TYPE
    {
        Striker,
        Ball,
        MathButton,
        MathBallon,
        SwitchGameBtn,
        WEAPON_MELEE,
        MathGun,
        MathBullet
        
    };

    public ObjectInGame.TYPE type;


    private void Awake()
    {
        instance = this;
        correctPos = this.transform.position;
        correctRot = this.transform.rotation;
    }

    private void OnEnable()
    {
        if (type == TYPE.MathBallon)
        {
            Vector3 initScale = transform.localScale;
            transform.localScale *=  0.2f;
            transform.DOScale(initScale,1f);
        }
        if(type == TYPE.MathBullet)
        {
            Invoke("AutoReturnToPool",3f);
        }
    }
    // Use this for initialization
    void Start() {

        InitObject();
        
        rigidBody = this.GetComponent<Rigidbody>();
        meshRenderer = this.GetComponent<MeshRenderer>();

        direct = new Vector3(1, 0, 0);


        currentSpeed = speed;
        initScaleY = this.transform.localScale.y;

    }

    // Update is called once per frame
    void Update() {
        UpdateObject();
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            OnStreamWrite(stream, info);
        }
        else
        {
            OnStreamReceive(stream, info);
        }
    }

    void OnStreamWrite(PhotonStream stream, PhotonMessageInfo info)
    {
        switch (type)
        {
            case TYPE.Striker:
                {
                    // We own this player: send the others our data
                    stream.SendNext(transform.position);
                    break;
                }
            case TYPE.WEAPON_MELEE:
            case TYPE.Ball:
                {
                    // We own this player: send the others our data
                    stream.SendNext(transform.position);
                    stream.SendNext(transform.rotation);
                    break;
                }
        }
    }

    void OnStreamReceive(PhotonStream stream, PhotonMessageInfo info)
    {
        switch (type)
        {
            case TYPE.Striker:
                {
                    // Network player, receive data
                    this.correctPos = (Vector3)stream.ReceiveNext();
                    break;
                }
            case TYPE.WEAPON_MELEE:
            case TYPE.Ball:
                {
                    // Network player, receive data
                    this.correctPos = (Vector3)stream.ReceiveNext();
                    this.correctRot = (Quaternion)stream.ReceiveNext();
                    break;
                }
        }
    }

#region event system
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(Player.instance.currentState != Player.PlayerState.PlayingGame)
         Player.instance.SetState(Player.PlayerState.Selecting);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Player.instance.currentState != Player.PlayerState.PlayingGame)
            Player.instance.SetState(Player.PlayerState.None);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
       
        switch (type)
        {
            case TYPE.MathButton:
                {
                    Debug.Log("Button clicked");

                    Player.instance.SetState(Player.PlayerState.PlayingGame);
                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(this.transform.DOScaleY(initScaleY * 0.5f, 0.25f).SetEase(Ease.OutSine));
                    sequence.Append(this.transform.DOScaleY(initScaleY, 0.5f).SetEase(Ease.OutBounce));

                    //answer of math game 
                    if (!MathGame.instance)
                    {
                        Debug.Log("MATH GAME NOT FOUND");
                        return;
                    }

                    // make move of math game
                    string answer = this.GetComponentInChildren<Text>().text.Trim();
                    int number = int.Parse(answer);
                    MathGame.instance.MakeTurn(number);
                    break;
                }
            case TYPE.SwitchGameBtn:
                {
                    Player.instance.SetState(Player.PlayerState.PlayingGame);
                    Debug.Log("SwitchGameBtn clicked");
                    Sequence sequence = DOTween.Sequence();
                    sequence.Append(this.transform.DOScaleY(initScaleY * 0.5f, 0.25f).SetEase(Ease.OutSine));
                    sequence.Append(this.transform.DOScaleY(initScaleY, 0.5f).SetEase(Ease.OutBounce));


                    if (PhotonNetwork.isMasterClient && MiniGameManager.instance.currentGamePrefab == null)
                    {

                        MiniGameManager.instance.currentGamePrefab = PhotonNetwork.Instantiate(extentGO.name, extentGO.transform.position, Quaternion.identity, 0);
                      
                    }

                   
                    break;
                }
            case TYPE.WEAPON_MELEE:
                {
                    //pickup item script need update
                    Pickup();
                    break;
                }

        }

    }
#endregion

    private void OnCollisionEnter(Collision collision)
    {
        switch (type)
        {
            case TYPE.Striker:
                {

                    break;
                }
            case TYPE.Ball:
                {

                    break;
                }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called: ");
        switch (type)
        {

            case TYPE.Striker:
                {
                    OnTriggerEnterStriker(other);
                    break;
                }
            case TYPE.Ball:
                {
                    OnTriggerEnterBall(other);
                    break;
                }
            case TYPE.WEAPON_MELEE:
                {
                    OnTriggerEnterAXE(other);
                    break;
                }
            case TYPE.MathBullet:
                {
                    OnTriggerEnterMathBullet(other);
                    break;
                }


        }
    }



    public void InitObject()
    {

    }

    public void UpdateObject()
    {
        switch (type)
        {
            case TYPE.Striker:
                {
                    UpdateStriker();
                    break;
                }
            case TYPE.Ball:
                {
                    UpdateBall();
                    break;
                }
            case TYPE.WEAPON_MELEE:
                {
                    UpdateMelee();
                    break;
                }
            case TYPE.MathGun:
                {
                    UpdateMathGun();
                    break;
                }
        }
    }

    #region common
    public bool SentPickup;
    public bool PickupIsMine;

    [PunRPC]
    public void SetParent(string parent)
    {
        GameObject parentGO = GameObject.Find("table");
        if (parentGO)
        {
            transform.SetParent(parentGO.transform);
            transform.localScale = new Vector3(1, 1, 1);
            if (PhotonNetwork.isMasterClient)
                transform.position = HockeyGame.instance.StrikerSpawnPoint[0].position;
            else
                transform.position = HockeyGame.instance.StrikerSpawnPoint[1].position;
        }


    }

    [PunRPC]
    public void PunPickup(PhotonMessageInfo msgInfo)
    {
        // when this client's RPC gets executed, this client no longer waits for a sent pickup and can try again
        if (msgInfo.sender.IsLocal) this.SentPickup = false;

        // In this solution, picked up items are disabled. They can't be picked up again this way, etc.
        // You could check "active" first, if you're not interested in failed pickup-attempts.
        if (!this.gameObject.GetActive())
        {
            return;     // makes this RPC being ignored
        }
        // if the RPC isn't ignored by now, this is a successful pickup. this might be "my" pickup and we should do a callback
        this.PickupIsMine = msgInfo.sender.IsLocal;
        //actions
        if (this.PickupIsMine)
        {
            photonView.TransferOwnership(PhotonNetwork.player.ID);
            Player.instance.SetState(Player.PlayerState.None);
            Player.instance.OnAttachItemToHand(this.transform);
        }
    }

    public void Pickup()
    {
        if (this.SentPickup)
        {
            // skip sending more pickups until the original pickup-RPC got back to this client
            return;
        }

        this.SentPickup = true;
        this.photonView.RPC("PunPickup", PhotonTargets.AllViaServer);
    }

    #endregion

    /// BEGIN HOCKEY GAME
    #region Striker
    void UpdateStriker()
    {
        if (photonView.isMine)
        {

            Vector3 pos = GvrPointerInputModule.CurrentRaycastResult.worldPosition;
            BoxCollider box = PhotonNetwork.isMasterClient ? HockeyGame.instance.validArea[0] : HockeyGame.instance.validArea[1];

            if (!FBUtils.PointInOABB(pos, box))
                return;

            this.transform.position = pos;
            pos.y = transform.position.y;
            //lastPos = pos;
            //rigidBody.MovePosition(pos);

            Ray a = new Ray(transform.position, transform.forward);
            Ray b;
            RaycastHit hit;

            if (Deflect(a, out b, out hit))
            {
                Debug.DrawLine(a.origin, hit.point);
                Debug.DrawLine(b.origin, b.origin + 3 * b.direction);
            }
        }
        else
        {
            transform.DOMove(correctPos, 0.2f);
            
        }

    }
    void OnTriggerEnterStriker(Collider other)
    {

    }

    bool Deflect(Ray ray, out Ray deflected, out RaycastHit hit)
    {

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 normal = hit.normal;
            Vector3 deflect = Vector3.Reflect(ray.direction, normal);

            deflected = new Ray(hit.point, deflect);
            return true;
        }

        deflected = new Ray(Vector3.zero, Vector3.zero);
        return false;
    }

    #endregion

    #region Ball
    Tweener tweenDoMove = null;
   
    void OnTriggerEnterBall(Collider other)
    {
        //check area
        if (other.gameObject == HockeyGame.instance.validArea[0].gameObject ||
            other.gameObject == HockeyGame.instance.validArea[1].gameObject)
            return;
        //check is goal
        if (other.gameObject == HockeyGame.instance.goals[0].gameObject)
        {
            HockeyGame.instance.photonView.RPC("AddScore2Players", PhotonTargets.AllViaServer, 1, 1);
            return;
        }
        else if (other.gameObject == HockeyGame.instance.goals[1].gameObject)
        {
            HockeyGame.instance.photonView.RPC("AddScore2Players", PhotonTargets.AllViaServer, 1, 0);
            return;
        }

        Vector3 contact = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position).normalized;
        direct = Vector3.Reflect(transform.position, contact);
        direct.y = 0;



        ObjectInGame objClass = other.GetComponent<ObjectInGame>();
        if (objClass)
        {
            if (objClass.instance.type == TYPE.Striker)
                currentSpeed =  (currentSpeed >= 2*speed) ? 2*speed : currentSpeed *= 1.7f;
        }
        else
        {
            currentSpeed *= 0.9f;
            
        }

        float timeToEnd = 0f;
        Vector3 targetPos = Vector3.zero;
       
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(direct), out hit))
        {
            Debug.Log("is raycast");
            Debug.DrawLine(transform.position, hit.point);
            float dist = Vector3.Distance(transform.position, hit.point);
            //Debug.Log(" time is :" + speed / dist);
            timeToEnd = dist / currentSpeed;
            targetPos = hit.point;
          //  transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Random.Range(15, 30), transform.rotation.eulerAngles.z);
            tweenDoMove = transform.DOMove(hit.point, timeToEnd).SetEase(Ease.Linear);
        }

        if (PhotonNetwork.isMasterClient)
            photonView.RPC("AddForceOverNetwork", PhotonTargets.AllViaServer
                , targetPos, timeToEnd, PhotonNetwork.ServerTimestamp);
    }

    float time = 0f;
    void UpdateBall()
    {
        //transform.Translate(direct * speed * Time.smoothDeltaTime);
        //time += Time.smoothDeltaTime;
        //if (time >= 0.2999f)
        //{
        //    time = 0;
        //    Debug.Log("my position is +" + transform.position);
        //}


    }

    [PunRPC]
    public void AddForceOverNetwork(Vector3 targetPos,float timeToEnd, int timestamp)
    {
        float delay = /*1.0f /*/ (PhotonNetwork.ServerTimestamp - timestamp)/1000f;
        Debug.Log("AddForceOverNetwork called, delay:" + delay + "-end time"+ timeToEnd);

        //if (delay > timeToEnd)
        //    return;
        //if (tweenDoMove != null)
        //    tweenDoMove.ChangeValues(transform.position,targetPos,timeToEnd - delay);

        //transform.DOKill();
        //delay <= timeToEnd
        if (delay <= timeToEnd)
        {
           
            tweenDoMove = transform.DOMove(targetPos, timeToEnd - delay).SetEase(Ease.Linear);
        }
        else
        {
            //tweenDoMove = null;
            //transform.DOKill();
            //transform.position = targetPos;
           
            transform.position = targetPos;
        }

    }

    
    Vector3 CalcNextPosition(float timestamp, Vector3 pos,float speed, Vector3 direct)
    {
        float dist = speed * timestamp;
        Ray a = new Ray(pos, transform.TransformDirection(direct));
        return a.GetPoint(dist);   
    }
    #endregion
    /// END HOCKEY GAME
   
    #region Button

    #endregion

    #region MELEE

    void UpdateMelee()
    {
        if (photonView.isMine)
        {

        }
        else
        {
            transform.DOMove(correctPos, 0.2f);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctRot, Time.deltaTime * 10f);
        }
           
    }
    void OnTriggerEnterAXE(Collider other)
    {
        GroupObject obj = other.GetComponent<GroupObject>();
        if(obj)
        {
            obj.UpdateHP(-10f);
        }
    }
    #endregion

    /// BEGIN MATH GAME

    #region MathGun
    private static float BULLETSTRENGTH = 20.0f;
    void StartMathGun()
    {
        
    }

   
    void UpdateMathGun()
    {
        if (!photonView.isMine)
            return;

        if (GvrControllerInput.ClickButtonUp)
        {
            //shoot
            GameObject bullet = FBPoolManager.instance.getPoolObject("Math_Gun_BULLET");
            if (bullet)
            {
                Transform shootMarker = transform.findChildRecursively("shoot_marker");
                if(shootMarker)
                    bullet.transform.position = shootMarker.position;
                else
                    bullet.transform.position = transform.position;

                bullet.transform.rotation = transform.rotation;

                Rigidbody rb = bullet.addMissingComponent<Rigidbody>();
                ObjectInGame script = bullet.addMissingComponent<ObjectInGame>();
                script.type = TYPE.MathBullet;

                rb.velocity = Vector3.zero; //reset force 
                bullet.SetActive(true);
                rb.AddForce(transform.forward * BULLETSTRENGTH, ForceMode.Impulse);


            }
        }
    }
    #endregion

    #region MathBullet
    void AutoReturnToPool()
    {
        //to return bullet to pool
        FBPoolManager.instance.returnObjectToPool(this.gameObject);
    }

    void OnTriggerEnterMathBullet(Collider other)
    {
        if(MathGame.instance.isSentMove())
        {
            Debug.Log("SENT MOVE RETURN");
            return;
        }

        if (other.gameObject.name.Contains("Math_Balloon"))
        {
            string answer = other.GetComponentInChildren<TextMesh>().text.Trim();
            int number = int.Parse(answer);
            MathGame.instance.MakeTurn(number);

            other.gameObject.SetActive(false);
        }
    }
    #endregion

    /// END MATH GAME
}
