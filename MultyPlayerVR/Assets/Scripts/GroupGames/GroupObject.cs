using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GroupObject : MonoBehaviour {

    public enum STATE
    {
        ACTIVE,
        DEACTIVE,
    }

    public enum ACTION
    {
        MOVE,
        IDLE,
        EAT
    }

    public int uID;
    public float hp = 100f;
    public STATE state;
    public GroupObjectManager parent;

    private Animator animator;

    public void SetState(STATE st)
    {
        if (state != st)
            state = st;
        else
            return;

        switch (st)
        {
            case STATE.ACTIVE:
                {
                    this.hp = 100f;
                    FBUtils.DoAnimScaleUp(this.gameObject);
                }
                break;
            case STATE.DEACTIVE:
                {
                    parent.OnDestroyObject(uID);
                }
                break;
            default:
                break;
        }
    }



    // Use this for initialization
    void Start () {
        SetState(STATE.ACTIVE);
        animator = this.GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {

        if (hp <= 0)
        {
            PlayDieFX();
            SetState(STATE.DEACTIVE);
        }

        //UpdateHitGround();
    }


    public void UpdateHP(float atk)
    {
        // hp += atk;
        FBParticleManager.PlayEffect(FBParticleManager.HIT_ANIMAL, 2, transform.position);
        parent.OnUpdateHp(uID, atk);
        if (hp <= 0)
        {
            SetState(STATE.DEACTIVE);
        }

       
    }

    public void OnSetIdle()
    {
        if(animator)
        {
            animator.SetFloat("Speed_f", 0f);
            animator.SetBool("Eat_b", false);
        }
        this.transform.DOComplete();
        

    }
    void UpdateHitGround()
    {
        float y = FBUtils.GetGroundYAxis(new Vector3(transform.position.x,100,transform.position.z));

        this.transform.position = new Vector3(this.transform.position.x,
            y, this.transform.position.z);
    }
    public void MoveTo(Vector3 pos,float duration)
    {
        float v = (float)( Vector3.Distance(transform.position, pos) / duration);
        if (animator)
        {
            animator.SetFloat("Speed_f",v);
            animator.SetBool("Eat_b", false);
        }

        
        this.transform.DOMove(pos, duration).SetEase(Ease.Linear).OnComplete( () => {
            animator.SetFloat("Speed_f", 0);
            animator.SetBool("Eat_b", false);
            //UpdateHitGround();
        } );

       this.transform.LookAt(pos);
    }

    public void ChangeAnim(ACTION action)
    {
        if(animator == null)
            return;
        switch (action)
        {
            case ACTION.MOVE:
                break;
            case ACTION.IDLE:
                {
                    animator.SetFloat("Speed_f", 0f);
                    animator.SetBool("Eat_b", false);
                }
                break;
            case ACTION.EAT:
                {
                    animator.SetFloat("Speed_f", 0f);
                    animator.SetBool("Eat_b", true);
                }
                break;
            default:
                break;
        }
    }

    private void PlayDieFX()
    {
        GameObject fx = null;
        switch (parent.species)
        {
            case GroupObjectManager.Species.RABBIT:
                  fx = FBParticleManager.GetEffect("rabbit_disable",2);
                break;
            case GroupObjectManager.Species.TREE:
                fx = FBParticleManager.GetEffect("tree_disable",2);
                break;
            case GroupObjectManager.Species.WHEAT:
                fx = FBParticleManager.GetEffect("tree_disable", 2);
                break;
            default:
                break;
        }
        if (fx)
        {
            fx.transform.position = this.transform.position;
            fx.SetActive(true);
        }
    }

}
