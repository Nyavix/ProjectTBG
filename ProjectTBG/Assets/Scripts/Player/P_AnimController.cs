using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_AnimController : MonoBehaviour
{
    Animator anim;

    P_Movement pMove;
    P_Combat pCombat;
    private bool onRight;

    public bool ikActive = false;
    public Transform leftHandObj = null;

    private bool falling = false;

    const string IdleAnimation = "Idle";
    const string MidAirAnimation = "MidAir";
    const string LandAnimation = "Land";
    const string RunAnimation = "Run";
    const string SprintAnimation = "Sprint";

    //Combat Animations
    const string B_Attack1Animation = "B_Attack 1";
    const string B_Attack2Animation = "B_Attack 2";
    const string B_Attack3Animation = "B_Attack 3";

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        pMove = GetComponentInParent<P_Movement>();
        pCombat = GetComponentInParent<P_Combat>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pMove.Grounded)
        {
            if (falling)
            {
                ChangeAnimationState(LandAnimation);
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 || 
                    (Mathf.Abs(pMove.Velocity.x) > pMove.walkSpeed && pMove.XInput != 0 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.6f))
                    falling = false;
            }
            else if(pCombat.inputReceived)
            {
                ChangeAnimationState(B_Attack1Animation);
                if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
                {
                    
                    pCombat.inputReceived = false;
                    pCombat.InputManager();
                    ChangeAnimationState(B_Attack2Animation);
                    Debug.Log(B_Attack2Animation);
                }
                    
            }
            else
            {
                    ChangeAnimationState(IdleAnimation);
            }
        }
        else
        {
            ChangeAnimationState(MidAirAnimation);
            falling = true;
        }


        //anim.SetBool("Grounded", pMove.Grounded);
        //anim.SetBool("Sliding", pMove.Sliding);
        anim.SetFloat("ySpeed", pMove.Velocity.y);

        anim.SetFloat("Speed", Mathf.Abs(pMove.Velocity.magnitude / pMove.runSpeed));

        //anim.SetFloat("Dir", pMove.Dir);
    }

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (anim)
        {

            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {

                // Set the right hand target position and rotation, if one has been assigned
                if (leftHandObj != null)
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
                }

            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }
        }
    }

    void ChangeAnimationState(string newState)
    {
        //Play the animation
        anim.Play(newState);
    }
}
