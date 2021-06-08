using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Dash : MonoBehaviour
{
    private P_Movement pMove;
    private CharacterController controller;
    public float dashSpeed;
    public float dashTime;

    private bool dashing;

    private bool dashed;
    public float dashCooldown = 1f;
    private float dashTimer = 1f;

    Vector3 moveDir;

    // Start is called before the first frame update
    void Start()
    {
        pMove = GetComponent<P_Movement>();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !dashed && dashTimer > dashCooldown)
        {
            StartCoroutine(Dash());
            dashed = true;
            dashTimer = 0;
        }

        if(pMove.Grounded) {
            dashed = false;
        }

        dashTimer += Time.deltaTime;
    }

    IEnumerator Dash()
    {
        dashing = true;
        pMove.SetMoveState(MovementState.Paused);
        float startTime = Time.time;
        if (pMove.XInput != 0)
        {
            moveDir = new Vector3((pMove.XInput == 1) ? 1 : -1, 0, 0);
        }
        else
        {
            moveDir = new Vector3((pMove.FacingRight) ? 1 : -1, 0, 0);
        }

        while (Time.time < startTime + dashTime)    
        {
            
            controller.Move(moveDir * dashSpeed * dashTime);
            yield return null;
        }
        pMove.SetMoveState(MovementState.groundMove);
        pMove.SetMoveDirection(Vector3.zero);
        dashing = false;
    }

    public bool Dashing
    {
        get { return dashing; }
    }
}
