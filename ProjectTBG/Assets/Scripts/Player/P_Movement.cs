using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Author: Nvikelo Nyathi
/// Date Made: 3/24/2020
/// 
/// Player movement script.
/// 
/// </summary>

//Made for the multiple diffrent states I want to keep seperate from eachother
public enum MovementState
{
    Paused,
    groundMove,
    wallSlide,
    LedgeHang
}

public class P_Movement : MonoBehaviour
{
    //--------------------------//
    //Variable start

    private CharacterController controller;
    private PlayerInput pInput;
    
    [SerializeField] private MovementState moveState = MovementState.groundMove;

    public LayerMask wallLayer;

    public float baseSpeed;
    public float walkSpeed = 4.0f;
    public float runSpeed = 8.0f;
    public float jumpSpeed = 10.0f;

    public float gravity = 18;
    private float gMultiplier;
    public float lowJumpMultiplier = 2.0f;
    public float fallMultiplier = 2.5f;

    bool walk;

    [Tooltip("Units that player can fall before a falling function is run. To disable, type \"infinity\" in the inspector.")]
    [SerializeField]
    private float fallingThreshold = 10.0f;

    [SerializeField]
    private float antiBumpFactor = .75f;
    private float rayDistance;
    private float slideLimit;

    [Tooltip("If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down.")]
    [SerializeField]
    private bool slideWhenOverSlopeLimit = false;

    [Tooltip("If checked and the player is on an object tagged \"Slide\", he will slide down it regardless of the slope limit.")]
    [SerializeField]
    private bool slideOnTaggedObjects = false;

    [Tooltip("How fast the player slides when on slopes as defined above.")]
    [SerializeField]
    private float gSlideSpeed = 12.0f;

    Vector2 input;
    float smoothDX;
    Vector2 mousePos;
    Vector3 moveDirection;
    Vector3 gunMomentumDir;
    Vector3 slideMoveDir;
    private Quaternion xRot;
    float slideSpeed = 10f;
    bool sliding = false;

    bool grounded;

    public float timeTillJumpTimer = 0.1f;
    float jumpTimer;
    public float groundTimeMax = 0.15f;
    float groundedTimer;
    bool stopJump;
    int wallDir;

    [Header("Slide")]
    public float slideMultiplier = 0.2f;
    public Vector2 slideJump = new Vector2(9,9);
    public Vector2 slideJumpHigh = new Vector2(10, 6);
    public Vector2 slideJumpLow = new Vector2(6, 10);
    int wallPerf;
    int pushOff;

    float ledgeDistance;
    Vector3 hitPoint;
    bool climbLedge;

    public Vector3 stepUpOffset;
    public Vector3 ledgeHangOffset;
    private bool facingRight;
    private RaycastHit s_Hit;
    private Vector3 contactPoint;
    private float fallStartLevel;
    private bool falling;
    public bool hasWeapon;
    bool shooting;
    private float angle;
    private bool canSlide;

    public Vector3 climbOffset;
    public Vector3 climbedOffset;

    //--------------------------//



    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

        pInput = GetComponentInParent<PlayerInput>();

        rayDistance = controller.height * .5f + controller.radius;
        slideLimit = 10 - .1f;
    }

    // Update is called once per frame
    void Update()
    {
        //Change move state to wallSlide if propper conditions met
        if (moveState != MovementState.wallSlide && moveState != MovementState.LedgeHang && !grounded && WallCheck())
        {
            moveState = MovementState.wallSlide;
        }

        //keep track of smooth value
        if (smoothDX < 0.25f && smoothDX > -0.25f && input.x == 0)
        {

            if (smoothDX < 0.01f && smoothDX > -0.01f)
                smoothDX = 0;
        }

        baseSpeed = (walk && !Sliding) ? walkSpeed : runSpeed;

        //Jumping based timers
        groundedTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;
    }

    //Used for physics based methods and functions
    private void FixedUpdate()
    {
        if (moveState == MovementState.LedgeHang)
            LedgeHang();

        if (moveState == MovementState.wallSlide)
        {
            wallSlide();
            grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
        }

        if (moveState == MovementState.groundMove)
        {
            Move();
            grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
        }

        if(grounded)
            smoothDX = Mathf.Lerp(smoothDX, input.x, slideSpeed * Time.deltaTime);
        else
            smoothDX = Mathf.Lerp(smoothDX, input.x, (slideSpeed * 1.75f) * Time.deltaTime);

        if (canSlide)
            angle = Mathf.Lerp(angle, (Vector3.Angle(s_Hit.normal, Vector3.up) > 45) ? angle : Vector3.Angle(s_Hit.normal, Vector3.up), 10 * Time.deltaTime);
        else
            angle = 0;

        transform.Find("Character").localEulerAngles = new Vector3(angle * ((facingRight) ? 1 : -1), (facingRight) ? 90 : -90, transform.localEulerAngles.z);
    }

    //Handles movement when the play is grounded
    private void Move()
    {
        Gravity();

        if (grounded)
        {
            sliding = false;

            // See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
            // because that interferes with step climbing amongst other annoyances
            if (Physics.Raycast(transform.position, -Vector3.up, out s_Hit, rayDistance))
            {
                if (Vector3.Angle(s_Hit.normal, Vector3.up) > slideLimit)
                {
                    
                        sliding = true;
                }
            }
            // However, just raycasting straight down from the center can fail when on steep slopes
            // So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
            else
            {
                Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out s_Hit);
                if (Vector3.Angle(s_Hit.normal, Vector3.up) > slideLimit)
                {
                        sliding = true;
                }
            }

            // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
            if (falling)
            {
                falling = false;
                if (transform.position.y < fallStartLevel - fallingThreshold)
                {
                    OnFell(fallStartLevel - transform.position.y);
                }
            }

            if (input.y < -.7f || Vector3.Angle(s_Hit.normal, Vector3.up) > controller.slopeLimit - 2.5f)
                canSlide = true;

            // If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on
            if (canSlide && ((sliding && slideWhenOverSlopeLimit) || (slideOnTaggedObjects && s_Hit.collider.tag == "Slide")))
            {
                Vector3 hitNormal = s_Hit.normal;
                slideMoveDir = Vector3.Lerp(slideMoveDir, new Vector3(hitNormal.x * gSlideSpeed, -hitNormal.y * gSlideSpeed, 0), 6 * Time.deltaTime);
                xRot = Quaternion.FromToRotation(Vector3.up, hitNormal);
            }
            else
            {

                if (slideMoveDir.magnitude < 0.2f)
                    slideMoveDir = Vector3.zero;
                else
                    slideMoveDir = Vector3.Lerp(slideMoveDir, Vector3.zero, (input.x != 0)? 3:2 * Time.deltaTime);
            }

            if (!Sliding)
                canSlide = false;

            moveDirection = new Vector3(smoothDX, -antiBumpFactor, 0);
            moveDirection += slideMoveDir;
            moveDirection += gunMomentumDir*.2f;
            moveDirection *= baseSpeed;

            groundedTimer = groundTimeMax;
            stopJump = false;
            gunMomentumDir = Vector3.Lerp(gunMomentumDir, Vector3.zero, 12 * Time.deltaTime);
        }
        else
        {
            // If we stepped over a cliff or something, set the height at which we started falling
            if (!falling)
            {
                falling = true;
                fallStartLevel = transform.position.y;
            }

            if (controller.velocity.magnitude < 12)
                gunMomentumDir = Vector3.Lerp(gunMomentumDir, Vector3.zero, 14 * Time.deltaTime);
            else
                gunMomentumDir = Vector3.zero;

            slideMoveDir = Vector3.Lerp(slideMoveDir, Vector3.zero, 0.5f * Time.deltaTime);
            angle = 0;

            moveDirection += gunMomentumDir;
            moveDirection.x += input.x * 0.025f;
        }

        if (hasWeapon || grounded)
            HandleFlip();

        if (jumpTimer > 0 && groundedTimer > 0)
        {
            moveDirection.y = jumpSpeed;
            jumpTimer = 0;
            groundedTimer = 0;
        }

        moveDirection.y -= (gravity * gMultiplier) * Time.deltaTime;

        wallPerf = 0;
    }

    //Handles gravity multiplier value
    void Gravity()
    {
        //faster falling
        if (moveDirection.y < 0)
        {
            gMultiplier = fallMultiplier;
        }
        else if (moveDirection.y > 0 && stopJump || CielingCheck() || shooting) 
        {
            gMultiplier = lowJumpMultiplier;
        }
        else
        {
            gMultiplier = 1.0f;
        }
    }

    //Check if player touched cieling and return true or false
    bool CielingCheck()
    {
        Debug.DrawRay(transform.position + (Vector3.up * controller.bounds.size.y), Vector3.up * 0.2f, Color.blue);
        return (Physics.Raycast(transform.position + (Vector3.up * controller.bounds.size.y), Vector3.up, 0.2f));
    }

    //Checks if player touched wall and return true or false
    bool WallCheck()
    {
        for (int i = 2; i > 0; i--)
        {
            Debug.DrawRay(transform.position + (Vector3.up * (controller.bounds.size.y - (0.3f * i))) + (Vector3.right * controller.bounds.extents.x), Vector3.right * 0.2f, Color.green);
            Debug.DrawRay(transform.position + (Vector3.up * (controller.bounds.size.y - (0.3f * i))) + (Vector3.left * controller.bounds.extents.x), Vector3.left * 0.2f, Color.green);

            bool right = (Physics.Raycast(transform.position + (Vector3.up * (controller.bounds.size.y - (0.3f * i))) + (Vector3.right * controller.bounds.extents.x), Vector3.right, 0.2f, wallLayer,QueryTriggerInteraction.Ignore));
            bool left = (Physics.Raycast(transform.position + (Vector3.up * (controller.bounds.size.y - (0.3f * i))) + (Vector3.left * controller.bounds.extents.x), Vector3.left, 0.2f, wallLayer, QueryTriggerInteraction.Ignore));

            if (right)
                wallDir = -1;
            if (left)
                wallDir = 1;

            if (right || left)
            {
                return true;
            }

            Debug.DrawRay(transform.position + (Vector3.up * (controller.bounds.size.y - (0.3f * i))) + (Vector3.right * controller.bounds.extents.x), Vector3.right * 0.2f, Color.red);
            Debug.DrawRay(transform.position + (Vector3.up * (controller.bounds.size.y - (0.3f * i))) + (Vector3.left * controller.bounds.extents.x), Vector3.left * 0.2f, Color.red);
        }

        return false;
    }

    //Checks if player by a ledge corner and return true or false
    bool ledgeCheck()
    {
        if(!Physics.Raycast(transform.position + (Vector3.right * -wallDir * controller.bounds.extents.x) + Vector3.up * 1.7f, Vector3.right * -wallDir, 0.4f))
        {
            Debug.DrawRay(transform.position + (Vector3.right * -wallDir * controller.bounds.extents.x) + Vector3.up * 1.7f, Vector3.right * -wallDir * 0.4f, Color.red);

            RaycastHit hit;

            if (Physics.Raycast(transform.position + (Vector3.right * -wallDir * controller.bounds.extents.x) + Vector3.up * 1.7f + ((Vector3.right * -wallDir) * 0.45f), Vector3.down, out hit, 0.6f))
            {
                Debug.DrawRay(transform.position + (Vector3.right * -wallDir * controller.bounds.extents.x) + Vector3.up * 1.7f + ((Vector3.right * -wallDir) * 0.7f), Vector3.down * 0.6f, Color.cyan);

                ledgeDistance = hit.distance;
                hitPoint = hit.point;

                return true;
            }
        }

        return false;
    }

    //Handle wallSlide based movement
    void wallSlide()
    {
        if(!WallCheck() || grounded)
        {
           moveState = MovementState.groundMove;
        }

        HandleFlip();

        smoothDX = 0;
        angle = 0;
        wallPerf++;

        //Handle diffrent jumps
        if (jumpTimer > 0 && wallPerf > 4)
        {
            if (wallDir > 0)
            {
                moveDirection = slideJump;

                moveState = MovementState.groundMove;
            }
            else if (wallDir < 0)
            {
                moveDirection = new Vector3(-slideJump.x, slideJump.y, 0);

                moveState = MovementState.groundMove;
            }

            wallPerf = 0;
        }

        //Handle pushing off a wall
        if (input.x == wallDir || input.y < 0)
        {
            pushOff++;
            if (pushOff == 10)
            {
                moveDirection.x = wallDir * 3;
                moveState = MovementState.groundMove;
                pushOff = 0;
            }
        }
        else pushOff = 0;

        //Reset horizontal movement if not zero
        if (moveDirection.x != 0)
            moveDirection.x = Mathf.Lerp(moveDirection.x, 0, 3 * Time.deltaTime);

        //Check if near a ledge
        if (ledgeCheck())
        {
            if (ledgeDistance < 0.2f && ledgeDistance != 0)
                moveState = MovementState.LedgeHang;
        }

        moveDirection += gunMomentumDir * .2f;

        gunMomentumDir = Vector3.Lerp(gunMomentumDir, Vector3.zero, 16 * Time.deltaTime);

        //Handle Gravity
        if (moveDirection.y > 0)
            moveDirection.y -= (gravity * slideMultiplier * 2.0f) * Time.deltaTime;
        else
            moveDirection.y -= (gravity * slideMultiplier) * Time.deltaTime;
    }

    //Hanle ledge hanging
    void LedgeHang()
    {
        if (!climbLedge)
            transform.position = new Vector3(ledgeHangOffset.x * ((facingRight) ? -1 : 1), ledgeHangOffset.y, ledgeHangOffset.z) + hitPoint;

        facingRight = wallDir > 0;
        slideMoveDir = Vector3.zero;
        gunMomentumDir = Vector3.zero;

        if (input.y > 0 || input.x == -wallDir || jumpTimer > 0)
        {
            climbLedge = true;
            transform.Find("Graphics").localPosition = climbOffset;
            moveDirection = Vector3.zero;
            controller.enabled = false;
        }
        else if(input.y < 0 || input.x == wallDir)
        {
            moveState = MovementState.groundMove;
            moveDirection = new Vector3(wallDir * 5,-4,0);
            ledgeDistance = 0;
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    public void EndLedgeHang()
    {
        moveState = MovementState.groundMove;
        transform.position = hitPoint + climbedOffset;
        ledgeDistance = 0;
        climbLedge = false;
        controller.enabled = true;
        facingRight = wallDir < 0;
    }

    void HandleFlip()
    {
        if (climbLedge)
            return;

        if (moveState == MovementState.wallSlide)
            facingRight = wallDir > 0;
        else if (hasWeapon)
        {
            if (pInput.currentControlScheme == "Gamepad")
                facingRight = Look.x > 0;
            else
                facingRight = Look.x > transform.position.x;
        }
       else
        {
            if (controller.velocity.x > 1f)
            {
                facingRight = true;
            }
            else if (controller.velocity.x < -1f)
            {
                facingRight = false;
            }
        }
    }

    public void AddVelocity(Vector2 vel)
    {
        if(Sliding)
            slideMoveDir = vel*.2f;
        else
            gunMomentumDir = vel*.2f;
    }

    // Store point that we're in contact with for use in FixedUpdate if needed
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        contactPoint = hit.point;
    }

    // This is the place to apply things like fall damage. You can give the player hitpoints and remove some
    // of them based on the distance fallen, play sound effects, etc.
    private void OnFell(float fallDistance)
    {
        print("Ouch! Fell " + fallDistance + " units!");
        SimpleCameraShakeInCinemachine.CameraShaker.ShakeScreen(0.05f, 3, 6);
    }

    public Vector2 Velocity
    {
        get { return controller.velocity; }
    }

    public bool Grounded
    {
        get { return grounded; }
    }

    public bool WallSlide
    {
        get { return moveState == MovementState.wallSlide; }
    }

    public bool WallHang
    {
        get { return moveState == MovementState.LedgeHang; }
    }

    public bool WallClimb
    {
        get { return climbLedge; }
    }

    public int Dir
    {
        get { return wallDir; }
    }

    public bool FacingRight
    {
        get { return facingRight; }
    }

    public bool Shooting
    {
        set { shooting = value; }
    }

    public Vector2 Look
    {
        get {
            var mPos = new Vector3(mousePos.x, mousePos.y, 7.0f);

            if (pInput.currentControlScheme == "Gamepad")
                return mPos;

            Vector3 objectPos = Camera.main.WorldToScreenPoint(GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Chest).position);
            mPos.x = mPos.x - objectPos.x;
            mPos.y = mPos.y - objectPos.y;
            return mPos; }
    }

    public float PAngle
    {
        get { return (canSlide)? Vector3.Angle(s_Hit.normal, Vector3.up) : 0; }
    }

    public bool HasWeapon
    {
        get { return hasWeapon; }
        set { hasWeapon = value; }
    }

    public bool Sliding
    {
        get { return slideMoveDir != Vector3.zero && input.x < 0.2f && input.x > -0.2f; }
    }

    //--------------------------//
    //Input Methods

    public void Jump(InputAction.CallbackContext context)
    {
        if (moveState == MovementState.Paused)
            return;

        if (context.action.phase == InputActionPhase.Started)
        {
            jumpTimer = timeTillJumpTimer;
            stopJump = false;
        }

        if(context.action.phase == InputActionPhase.Canceled)
        {
            stopJump = true;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (moveState == MovementState.Paused)
            return;

        input = context.ReadValue<Vector2>();
    }

    public void MousePos(InputAction.CallbackContext context)
    {
        if (moveState == MovementState.Paused)
            return;
        if (pInput.currentControlScheme == "Gamepad")
        {
            if(context.ReadValue<Vector2>() != Vector2.zero)
                mousePos = context.ReadValue<Vector2>();
        }
        else
            mousePos = context.ReadValue<Vector2>();
    }

    public void Walk(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Started)
        {
            walk = true;
        }

        if (context.action.phase == InputActionPhase.Canceled)
        {
            walk = false;
        }
    }

    //--------------------------//

}
