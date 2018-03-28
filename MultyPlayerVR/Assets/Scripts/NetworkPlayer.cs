using DG.Tweening;
using UnityEngine;

public class NetworkPlayer : Photon.MonoBehaviour {
    GameObject playerGO;
    Transform cameraTransform;
    Transform controllerTransform;
    public Transform visualHead;
    public Transform visualHandTransform;
    public Transform visualLowerJaw;
    public float visualLowerJawScaleY;
    public float range = 100f;
    public float health;

    private Vector3 lowerJawInitPos;
    public Transform handItem;


#region Voice Regconition
    public AudioSource audioSource;
    public int numSample = 1024;
    public float[] samples;

    float GetAveragedVolume()
    {
        float result = 0f;
        if(audioSource.isPlaying)
        {
            for(int chanel = 0;chanel < 2;chanel++)
            {
                audioSource.GetOutputData(samples, chanel);
                for(int i = 0; i< numSample;i++)
                {
                    result += Mathf.Abs(samples[i]);
                }
            }
        }
        return result / numSample;
    }

    #endregion

  

    // Use this for initialization
    void Start () {
        playerGO = GameObject.Find("Player");
        lowerJawInitPos = visualLowerJaw.transform.localPosition;
        visualLowerJawScaleY = visualLowerJaw.transform.localScale.y;
        health = 100f;
        foreach (Transform child in playerGO.transform)
        {
            if(child.name == "Main Camera")
            {
                cameraTransform = child;
            }
            else if(child.name == "GvrControllerPointer")
            {
                controllerTransform = child.GetChild(0); // is controller 
            }

            
           
        }

        //enable voice recorder only if it is mine;
        if(photonView.isMine)
        {
            this.transform.GetComponent<PhotonVoiceRecorder>().enabled = true;
            //save this avatar to player prefab
            Player.instance.visualPlayer = gameObject;
            Player.instance.networkPlayer = this;
            //if (playerGO)
            //    transform.SetParent(playerGO.transform);
            //transform.localPosition = Vector3.zero;


            //samples = new float[numSample];
            //audioSource = transform.GetComponent< AudioSource > ();
            //audioSource.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
            //while (!(Microphone.GetPosition(null) > 0)) { }
            //audioSource.Play();
          

        }

    }
	

    public void AttachHandItem(Transform item)
    {
        handItem = item;
        item.transform.SetParent(visualHandTransform);
        item.transform.localPosition = Vector3.zero; 
    }

	// Update is called once per frame
	void Update () {

      
       
        //if(!photonView.isMine)
        //{
        //    float volume = GetAveragedVolume();
        //    visualLowerJaw.position = lowerJawInitPos - transform.up * volume;
        //}
        if (photonView.isMine)
        {

            //visualLowerJaw.localPosition = new Vector3(lowerJawInitPos.x + MicInput.instance.MicLoudness / 15f, lowerJawInitPos.y, lowerJawInitPos.z);
            visualLowerJaw.transform.DOScaleY(visualLowerJawScaleY + MicInput.instance.MicLoudness / 4f, 0.2f);
            GvrBasePointer laserPointerImpl = (GvrBasePointer)GvrPointerInputModule.Pointer;


            // Lerping smooths the movement
            if (cameraTransform)
            {
                visualHead.rotation = cameraTransform.rotation;
            }
            if(controllerTransform)
            {
                visualHandTransform.position = controllerTransform.position;
                visualHandTransform.rotation = controllerTransform.rotation;
               // visualHandTransform.position = controllerTransform.position;
            }
            if (handItem)
            {
                handItem.position = controllerTransform.position;
                handItem.rotation = controllerTransform.rotation;
                // visualHandTransform.position = controllerTransform.position;
            }

        }

    }


    #region RPC
    [PunRPC]
    public void SendAttachItemToHand(string itemName, int parentViewId)
    {
        itemName = itemName.Replace("(Clone)", "");
        PhotonView view = PhotonView.Find(parentViewId);
        if (view)
        {
            Debug.Log("itemName: "+ itemName);
            GameObject go = FBPoolManager.instance.getPoolObject(itemName);
            if (go == null)
            {
                Debug.Log("this bject not in pool !!!!!");
                return;
            }
            Transform rightHand = view.gameObject.transform.findChildRecursively("Hand_Right_jnt");
            //reset old attached if have
            foreach (Transform child in rightHand.transform)
            {
                FBPoolManager.instance.returnObjectToPool(child.gameObject);
            }

            go.transform.SetParent(rightHand);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.gameObject.layer = 2; //ignore ray cast
            Player.instance.currentHandItem = go;
            go.SetActive(true);
        }
    }

    [PunRPC]
    public void SendRemoveHandItem(int parentViewId)
    {
        PhotonView view = PhotonView.Find(parentViewId);
        if (view)
        {
            Transform rightHand = view.gameObject.transform.findChildRecursively("Hand_Right_jnt");
            //reset old attached if have
            foreach (Transform child in rightHand.transform)
            {
                FBPoolManager.instance.returnObjectToPool(child.gameObject);
            }
        }
    }

    public void SendFakeCollectItem(int playerViewId, int itemId, string itemDockname)
    {
        PhotonView view = PhotonView.Find(playerViewId);
        if (view)
        {
            itemDockname = itemDockname.Replace("(Clone)", "");
            Transform visualPlayer = view.gameObject.transform;

            Item finalItem =  ItemDatabase.instance.GetItemByID(itemId);
           
            if (finalItem == null )
            {
                return;
            }

            //START FAKE COLLECT
            GameObject itemObj = FBPoolManager.instance.getPoolObject(finalItem.prefabName);
            Vector3 dockPos = GameObject.Find(itemDockname).transform.position;

            itemObj.transform.position = dockPos;
            itemObj.SetActive(true);
            itemObj.transform.DOJump(visualPlayer.transform.position, 2f, 1, 1).OnComplete(() =>
            {
                FBPoolManager.instance.returnObjectToPool(itemObj);
            });



        }
    }
    #endregion


}
