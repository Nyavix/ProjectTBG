using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_AnimController : MonoBehaviour
{
    Animator anim;

    P_Movement pMove;
    P_Dash pDash;
    private bool onRight;

    public bool ikActive = false;
    public Transform leftHandObj = null;

    private bool falling = false;

    const string IdleAnimation = "Idle";
    const string MidAirAnimation = "MidAir";
    const string LandAnimation = "Land";
    const string SprintAnimation = "Sprint";
    const string DashAnimation = "Dash";

    private float returnIdle;
    private static float idleReturnTime = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        pMove = GetComponentInParent<P_Movement>();
        pDash = GetComponentInParent<P_Dash>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pDash.Dashing)
        {
            ChangeAnimationState(DashAnimation);
            returnIdle = idleReturnTime - 0.05f;
        }
        else if (pMove.Grounded)
        {
            if (falling)
            {
                ChangeAnimationState(LandAnimation);
                falling = false;
            }

            if(returnIdle > idleReturnTime)
                ChangeAnimationState(IdleAnimation);

            returnIdle += Time.deltaTime;
        }
        else
        {
            ChangeAnimationState(MidAirAnimation);
            falling = true;
            returnIdle = 0;
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
