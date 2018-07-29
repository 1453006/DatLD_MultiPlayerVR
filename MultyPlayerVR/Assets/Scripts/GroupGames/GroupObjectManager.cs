using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupObjectManager : Photon.MonoBehaviour
{
    public const float RANGE_MOVE = 10f;
    float timer = 0f;
    public enum TYPE
    {
        IDLE,
        MOVEABLE
    };

    public enum Species
    {
        RABBIT,
        TREE,
        WHEAT
    }


    public TYPE groupType;
    public Species species;
    public List<GameObject> listChildObj = new List<GameObject>();

    public string receiveItemName = null;
    public float TimeReset;
    public float Range;
    // Use this for initialization
    void Start () {
        this.gameObject.addMissingComponent<PhotonView>();
        InitItemData();
        InitChilds();
        switch (groupType)
        {
            case TYPE.IDLE:
                break;
            case TYPE.MOVEABLE:
                StartMoveable();
                break;
            default:
                break;
        }

    }
	
    void InitItemData()
    {
        Item dataSrc = GroupGameDatabase.instance.getItemData(this.gameObject.name);
        if (dataSrc.Id != -1)
        {
            //get prefab name of receive item => pool
            int receviveItemId = dataSrc.ReceiveItemId;
            receiveItemName  = GroupGameDatabase.instance.GetItemPrefabName(receviveItemId);
        }
    }
	// Update is called once per frame
	void Update () {
        switch (groupType)
        {
            case TYPE.IDLE:
                UpdateIdle();
                break;
            case TYPE.MOVEABLE:
                UpdateMoveable();
                break;
            default:
                break;
        }
    }


    void InitChilds()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject tmp = transform.GetChild(i).gameObject;
            //Rigidbody
            Rigidbody rb = tmp.addMissingComponent<Rigidbody>();
            rb.isKinematic = true;
            //Collider
            if (groupType != TYPE.MOVEABLE)
            {
               //MeshCollider mCollider =  tmp.addMissingComponent<MeshCollider>();
               // mCollider.convex = true;
               // mCollider.isTrigger = true;
            }
            //script
            GroupObject script = tmp.addMissingComponent<GroupObject>();
            script.uID = i;
            script.parent = this;
            listChildObj.Add(tmp);
        }

    }
    #region IDLE

    void UpdateIdle()
    {
        timer += Time.deltaTime;
        if (timer >= TimeReset)
        {
            timer = 0f;
            OnSendRefreshTree();
        }
    }

    void OnSendRefreshTree()
    {
        if (!PhotonNetwork.isMasterClient)
            return;
    
            photonView.RPC("SendSetActiveAll", PhotonTargets.AllBufferedViaServer);
    }

    public void OnDestroyObject(int uID)
    {
        photonView.RPC("DestroyObject", PhotonTargets.AllViaServer, uID);
    }

    public void OnUpdateHp(int uID, float atk)
    {
        CheckLocalAndReturnReward(uID, atk);
        photonView.RPC("UpdateChildHP", PhotonTargets.AllViaServer, uID,atk);
    }
    #endregion

    #region Moveable
    
    void StartMoveable()
    {
        GenChildAction();
    }

    public void UpdateMoveable()
    {
       timer += Time.deltaTime;
        if(timer >= TimeReset)
        {
            timer = 0f;
            GenChildAction();
        }

    }

    void GenChildAction()
    {
        if (!PhotonNetwork.isMasterClient)
            return;
        for (int i = 0; i < listChildObj.Count; i++)
        {
            int actionNum = FBUtils.RandomEnumValue<GroupObject.ACTION>();
            GroupObject.ACTION action = (GroupObject.ACTION)actionNum;
            SetAction(action,i);
        }
      
    }

    void SetAction(GroupObject.ACTION action, int childIndex)
    {
        switch (action)
        {
            case GroupObject.ACTION.MOVE:
                {
                    //SendMoveTo(GenNextPosition(),TimeReset/2f,childIndex);
                    Vector3 p = GenNextPosition();
                    photonView.RPC("SendMoveTo", PhotonTargets.AllViaServer, p.x, p.y, p.z, TimeReset / 2f, childIndex);
                }
                break;
            case GroupObject.ACTION.IDLE:
            case GroupObject.ACTION.EAT:
                {
                    //SendChangeAnim(action, childIndex);
                    photonView.RPC("SendChangeAnim", PhotonTargets.AllViaServer, action, childIndex);
                }
                break;
            default:
                break;
        }
    }

    Vector3 GenNextPosition()
    {
        float x = transform.position.x + Random.Range(-RANGE_MOVE, RANGE_MOVE);
        float y = transform.position.y;
        float z = transform.position.z + Random.Range(-RANGE_MOVE, RANGE_MOVE);
        return new Vector3(x,y,z);
    }

    void CheckLocalAndReturnReward(int uID,float atk)
    {
        GroupObject script = listChildObj[uID].GetComponent<GroupObject>();
        if (script && (script.hp + atk <= 0))
        {
            //return reward here
            GameObject obj = FBPoolManager.instance.getPoolObject(this.receiveItemName);
            
            
            obj.transform.position = script.gameObject.transform.position;
            obj.SetActive(true);
            //do animation fly
            FBUtils.DoAnimJumpOut(obj);
        }
    }
    #endregion


    #region RPC
    [PunRPC]
    public void DestroyObject(int uID)
    {
        //get receive Item
        Item itemData = listChildObj[uID].gameObject.GetComponent<Item>();
        if(itemData != null)
        {
            
        }
        listChildObj[uID].gameObject.SetActive(false);
    }

    [PunRPC]
    public void UpdateChildHP(int uID,float atk)
    {
        GroupObject script = listChildObj[uID].GetComponent<GroupObject>();
        if (script)
        {
            script.hp += atk;
            script.OnSetIdle();
        }
    }

    [PunRPC]
    public void SendMoveTo(float x, float y,float z, float duration,int childIndex)
    {
        GroupObject script = listChildObj[childIndex].GetComponent<GroupObject>();
        script.MoveTo(new Vector3(x,y,z),duration);
    }

    [PunRPC]
    public void SendChangeAnim(GroupObject.ACTION action, int childIndex)
    {
        GroupObject script = listChildObj[childIndex].GetComponent<GroupObject>();
        script.ChangeAnim(action);
    }

    [PunRPC]
    public void SendSetActiveAll()
    {
        for (int i = 0; i < listChildObj.Count; i++)
        {
            if(!listChildObj[i].GetActive())
            {
                listChildObj[i].GetComponent<GroupObject>().SetState(GroupObject.STATE.ACTIVE);
               
            }
        }
    }

  
    #endregion

}
