using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform camera;

    private float groundCheckRadius = 0.3f;
    private float speed = 8f;
    private float turnSpeed = 1500f;
    private float jumpForce = 500f;

    private Rigidbody rigidbody;
    private Vector3 direction;
    private GravityBody gravityBody;
    private Animator animator;
    private Transform meshTransform;
    private string state;
    private bool inAir;
    private bool isRunning;
    private string currentAnim;
    private bool wasGrounded;
    private float landingTimer = 0f;
    private float landingDuration = 2f;
    private float attackTimer = 0f;
    private float attackDuration = 1f;

    [HideInInspector] public bool isLaunching = false;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        gravityBody = GetComponent<GravityBody>();
        animator = GetComponentInChildren<Animator>();
        
        meshTransform = transform.Find("player_v3");
        state = "idle";
        currentAnim = "idle";
        inAir = false;
        isRunning = false;
        wasGrounded = false;
    }

    void Update()
    {
        if (isLaunching) return;

        direction = new Vector3(
            Input.GetAxisRaw("Horizontal"), 0f,
            Input.GetAxisRaw("Vertical")
        ).normalized;

        bool isGrounded = Physics.CheckSphere(
            groundCheck.position, groundCheckRadius, groundMask
        );

        if (inAir && isGrounded)
        {
            inAir = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            inAir = true;
            rigidbody.AddForce(
                -gravityBody.GravityDirection * jumpForce,
                ForceMode.Impulse
            );
        }
        
        if (Input.GetKeyDown(KeyCode.E) && attackTimer <= 0f)
        {
            attackTimer = attackDuration;
        }
        else if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
        bool isAttacking = attackTimer > 0f;
        
        
        bool justLanded = isGrounded && !wasGrounded;
        wasGrounded = isGrounded;

        if (justLanded)
        {
            landingTimer = landingDuration;
        }
        else if (landingTimer > 0f)
        {
            landingTimer -= Time.fixedDeltaTime;
        }

        bool isLanding = landingTimer > 0f;
        
        state = getNextAnimationState(isGrounded, isRunning, rigidbody.linearVelocity.y, isLanding, isAttacking);
        playAnimation();
        updateDirection();
    }

    void FixedUpdate()
    {
        if (isLaunching) return;

        bool isRunning = direction.magnitude > 0.1f;
        this.isRunning = isRunning;

        if (isRunning)
        {
            Vector3 direction = transform.forward * this.direction.z;
            rigidbody.MovePosition(
                rigidbody.position + direction * (speed * Time.fixedDeltaTime)
            );

            Quaternion rightDirection = Quaternion.Euler(
                0f, this.direction.x * (turnSpeed * Time.fixedDeltaTime), 0f
            );
            Quaternion newRotation = Quaternion.Slerp(
                rigidbody.rotation,
                rigidbody.rotation * rightDirection,
                Time.fixedDeltaTime * 3f
            );
            rigidbody.MoveRotation(newRotation);
        }
    }

    void updateDirection()
    {
        Vector3 meshAngles = meshTransform.eulerAngles;
        meshAngles.x = camera.eulerAngles.x;
        meshTransform.eulerAngles = meshAngles;
    }

    string getNextAnimationState(bool isGrounded, bool isRunning, float verticalVelocity, bool isLanding, bool isAttacking)
    {

        if (isAttacking) return "attacking";
        //if (isLanding) return "landing";
        if (!isGrounded)
        {
            return "jumping";
        }
        
        if (isRunning) return "running";
        return "idle";
    }

    void playAnimation()
    {
        if (currentAnim == state) return;
        currentAnim = state;
        
        switch (state)
        {
            case "landing":
                animator.Play("Armature|Land");
                break;
            case "running":
                animator.Play("Armature|Walk");
                break;
            case "jumping":
                animator.Play("Armature|Jump");
                break;
            case "idle":
                animator.Play("Armature|Idle");
                break;
            case "attacking":
                animator.Play("Armature|Attack");
                break;
        }
    }
}