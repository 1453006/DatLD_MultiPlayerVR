using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BasePopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public GameObject belongTo = null;
    public bool isFollowPlayer;
    //duration == -1 is forever
    public float duration;
    public Text text;

    ///start for timer
    public Image fillBar;
    float timer = 0f;
    /// Swipe actions return the index of the selected menu item.
    public delegate void OnCompleteTimer();
    public event OnCompleteTimer eventCompleteTimer;

    //end for timer

    private void Awake()
    {
        text = this.GetComponentInChildren<Text>();
    }
    // Use this for initialization
    void Start() {
        if (belongTo)
        {
            transform.SetParent(belongTo.transform);
            BoxCollider collider = belongTo.gameObject.GetComponent<BoxCollider>();
            if (collider)
            {
                float height = collider.size.y;
                transform.localPosition = new Vector3(0, height, 0);
            }
            else
            {
                //transform.localPosition = Vector3.zero;
            }

        }

        ShowPopup();
    }

    public virtual void ShowPopup()
    {
        isFollowPlayer = true;
    }
    public void SetValues(GameObject belong, bool isFollow, float lifetime)
    {
        this.belongTo = belong;
        this.isFollowPlayer = isFollow;
        this.duration = lifetime;
    }

    public void SetText(string value)
    {
        if (text)
            text.text = value;
    }
    // Update is called once per frame
    void Update() {
        if (isFollowPlayer)
            transform.faceToMainCamera();

        timer += Time.deltaTime;

        //contain fil bar => is timer 
        if (fillBar)
        {
            float displayValue = duration - timer;
            if (displayValue <= 0)
            {
                if (eventCompleteTimer != null)
                    eventCompleteTimer.Invoke();

                text.text = "0";
                fillBar.fillAmount = 0f;
            }
            else
            {
                text.text = ((int)displayValue).ToString();
                fillBar.fillAmount = (float)(displayValue / duration);
            }
        }
        if (duration != -1 && timer >= duration)
        {
            timer = 0f;
            FBPoolManager.instance.returnObjectToPool(this.gameObject, FBPoolManager.POOLTYPE.UI);
        }
    }

    #region EventSystem
    public void OnPointerEnter(PointerEventData eventData)
    {
        Player.instance.SetState(Player.PlayerState.Selecting);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Player.instance.SetState(Player.PlayerState.None);
    }
    #endregion


}
