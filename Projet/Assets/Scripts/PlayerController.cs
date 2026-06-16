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

    [HideInInspector] public bool isLaunching = false;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        gravityBody = GetComponent<GravityBody>();
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

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rigidbody.AddForce(
                -gravityBody.GravityDirection * jumpForce,
                ForceMode.Impulse
            );
        }
    }

    void FixedUpdate()
    {
        if (isLaunching) return;

        bool isRunning = direction.magnitude > 0.1f;

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
}