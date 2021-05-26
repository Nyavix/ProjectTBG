using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_FeetFX : MonoBehaviour
{
    private P_Movement pMove;
    private Animator anim;

    public GameObject feetFXPrefab;

    public GameObject landFXPrefab;

    public GameObject JumpFXPrefab;
    private Transform jumpFX;
    private float jumpTimer;

    private bool falling;

    private bool slidOnWall;

    int rotDir = 1;

    public Transform leftFoot;
    public Transform rightFoot;
    private bool leftStep = false;
    private bool rightStep = false;

    float slideTimer;

    public AudioClip[] metalFSteps;
    public AudioClip[] concreteFSteps;
    public AudioClip[] woodFSteps;


    public AudioClip jumpSound;

    // Start is called before the first frame update
    void Start()
    {
        pMove = GetComponentInParent<P_Movement>();
        anim = pMove.GetComponentInChildren<Animator>();

        leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    // Update is called once per frame
    void Update()
    {
        FeetCheck();

        if (!pMove.Grounded)
        {
            if (!falling)
            {
                if (pMove.Velocity.y > 0)
                    GetComponent<AudioSource>().PlayOneShot(jumpSound);
                falling = true;
            }
        }

        if (!pMove.Grounded && pMove.Velocity.y > 0)
        {
            if(jumpFX == null && jumpTimer < 0.25f)
            {
                jumpFX = Instantiate(JumpFXPrefab, transform.position, transform.rotation).transform;
                rotDir *= -1;
            }
            else if(jumpTimer < 0.25f)
            {
                jumpFX.Rotate(0, (540 * rotDir) * Time.deltaTime, 0);
                jumpFX.position = transform.position;
            }

            if (pMove.WallSlide)
            {
                slidOnWall = true;
                jumpTimer = 3;
            }

            if (slidOnWall && !pMove.WallSlide && jumpTimer > 0.01f && !pMove.WallHang)
            {
                jumpTimer = 0;
                GetComponent<AudioSource>().PlayOneShot(jumpSound);
                slidOnWall = false;
            }

            jumpTimer += Time.deltaTime;
        }
        else
        {
            jumpTimer = 0;

            slidOnWall = false;

            if (jumpFX != null)
            {
                jumpFX = null;
            }
        }

        if (pMove.Grounded)
        {
            if (falling)
            {
                Instantiate(landFXPrefab, transform.position, transform.rotation);

                falling = false;
            }
        }

        if (pMove.Sliding)
        {
            if(slideTimer > 0.1f)
            {
                Instantiate(feetFXPrefab, transform.position, transform.rotation);
                slideTimer = 0;
            }

            slideTimer += Time.deltaTime;
        }
    }

    void FeetCheck()
    {
        RaycastHit lFHit;
        RaycastHit rFHit;

        if(Physics.Raycast(leftFoot.position, Vector3.down, out lFHit, 0.2f))
        {
            if (!leftStep)
            {
                Instantiate(feetFXPrefab, leftFoot.position, transform.rotation);
                GetComponent<AudioSource>().PlayOneShot(metalFSteps[Random.Range(0, metalFSteps.Length - 1)]);
                leftStep = true;
            }

            Debug.DrawLine(leftFoot.position, lFHit.point, Color.green);
        }
        else
        {
            leftStep = false;

            Debug.DrawRay(leftFoot.position, Vector3.down * 0.2f, Color.red);
        }

        if (Physics.Raycast(rightFoot.position, Vector3.down, out rFHit, 0.2f))
        {
            if (!rightStep)
            {
                Instantiate(feetFXPrefab, rightFoot.position, transform.rotation);
                GetComponent<AudioSource>().PlayOneShot(metalFSteps[Random.Range(0, metalFSteps.Length - 1)]);
                rightStep = true;
            }

            Debug.DrawLine(rightFoot.position, rFHit.point, Color.green);
        }
        else
        {
            rightStep = false;
            
            Debug.DrawRay(rightFoot.position, Vector3.down * 0.2f, Color.red);
        }
    }
}
