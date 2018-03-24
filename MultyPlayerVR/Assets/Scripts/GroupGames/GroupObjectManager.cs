using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupObjectManager : Photon.MonoBehaviour
{
    public const float RANGE_MOVE = 10f;
    public enum TYPE
    {
        IDLE,
        MOVEABLE
    };

    public TYPE groupType;
    public List<GameObject> listChildObj = new List<GameObject>();

    public float TimeReset;
    public float Range;
    // Use this for initialization
    void Start () {
        this.gameObject.addMissingComponent<PhotonView>();

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
	
	// Update is called once per frame
	void Update () {
        switch (groupType)
        {
            case TYPE.IDLE:
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
            if(groupType != TYPE.MOVEABLE)
                tmp.addMissingComponent<BoxCollider>();
            //script
            GroupObject script = tmp.addMissingComponent<GroupObject>();
            script.uID = i;
            script.parent = this;
            listChildObj.Add(tmp);
        }

    }
    #region IDLE

    public void OnDestroyObject(int uID)
    {
        photonView.RPC("DestroyObject", PhotonTargets.AllViaServer, uID);
    }
    #endregion

    #region Moveable
    float timer = 0f;
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
                    SendMoveTo(GenNextPosition(),TimeReset/2f,childIndex);
                }
                break;
            case GroupObject.ACTION.IDLE:
                {
                    SendChangeAnim(action,childIndex);
                }
                break;
            case GroupObject.ACTION.EAT:
                {
                    SendChangeAnim(action, childIndex);
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
            script.hp += atk;
    }

    [PunRPC]
    public void SendMoveTo(Vector3 pos, float duration,int childIndex)
    {
        GroupObject script = listChildObj[childIndex].GetComponent<GroupObject>();
        script.MoveTo(pos,duration);
    }

    [PunRPC]
    public void SendChangeAnim(GroupObject.ACTION action, int childIndex)
    {
        GroupObject script = listChildObj[childIndex].GetComponent<GroupObject>();
        script.ChangeAnim(action);
    }
    #endregion

}
