using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_AnimController : MonoBehaviour
{
    Animator anim;

    P_Movement pMove;
    private bool onRight;

    public bool ikActive = false;
    public Transform leftHandObj = null;

    public Vector3 stomachRotation;

    public GameObject emptyAmmoPrefab;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        pMove = GetComponentInParent<P_Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Grounded", pMove.Grounded);
        anim.SetBool("Sliding", pMove.Sliding);
        anim.SetFloat("ySpeed", pMove.Velocity.y);

        if (pMove.HasWeapon)
        {
            var speed = Mathf.Clamp((pMove.Velocity.x / pMove.runSpeed) * ((pMove.FacingRight) ? 1 : -1), -1, 1);

            anim.SetFloat("Speed", speed, 0.01f, Time.deltaTime);
        }
        else if (pMove.WallHang || pMove.WallClimb)
            anim.SetFloat("Speed", 0);
        else
            anim.SetFloat("Speed", Mathf.Abs(pMove.Velocity.magnitude / pMove.runSpeed), 0.01f, Time.deltaTime);

        anim.SetBool("WallHang", pMove.WallSlide || pMove.WallHang);
        anim.SetBool("LedgeHang", pMove.WallHang);
        anim.SetBool("LedgeClimb", pMove.WallClimb);
        anim.SetFloat("Dir", pMove.Dir);

        anim.SetBool("hasWeapon", pMove.hasWeapon && !pMove.WallClimb);

        Vector3 mousePos = pMove.Look;

        onRight = mousePos.x > transform.position.x;

        if (!pMove.FacingRight)
            mousePos.x *= -1;

        var angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        anim.SetFloat("Look", (pMove.HasWeapon) ? angle + (pMove.PAngle * ((pMove.FacingRight)? 1 : -1)) : 0, 0.01f, Time.deltaTime);
    }

    public void EndLedgeHang()
    {
        pMove.EndLedgeHang();
    }

    public void ThrowAmmo()
    {
        var deadAmmoCell = Instantiate(emptyAmmoPrefab, anim.GetBoneTransform(HumanBodyBones.LeftHand).position, Quaternion.Euler(Vector3.zero));
        deadAmmoCell.GetComponent<Rigidbody>().AddForce(Vector3.right * ((pMove.FacingRight) ? -1 : 1) * 4 + Vector3.up * 2, ForceMode.Impulse);
    }

    public void Reloading(float reloadSpeed, bool reloading)
    {
        anim.SetFloat("ReloadSpeed", reloadSpeed);
        anim.SetBool("Reload", reloading);
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
}
