using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Transform _cam;

    private float _groundCheckRadius = 0.3f;
    private float _speed = 8f;
    private float _turnSpeed = 1500f;
    private float _jumpForce = 500f;

    private Rigidbody _rigidbody;
    private Vector3 _direction;
    private GravityBody _gravityBody;

    [HideInInspector] public bool isLaunching = false;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityBody = GetComponent<GravityBody>();
    }

    void Update()
    {
        if (isLaunching) return;

        _direction = new Vector3(
            Input.GetAxisRaw("Horizontal"), 0f,
            Input.GetAxisRaw("Vertical")
        ).normalized;

        bool isGrounded = Physics.CheckSphere(
            _groundCheck.position, _groundCheckRadius, _groundMask
        );

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            _rigidbody.AddForce(
                -_gravityBody.GravityDirection * _jumpForce,
                ForceMode.Impulse
            );
        }
    }

    void FixedUpdate()
    {
        if (isLaunching) return;

        bool isRunning = _direction.magnitude > 0.1f;

        if (isRunning)
        {
            Vector3 direction = transform.forward * _direction.z;
            _rigidbody.MovePosition(
                _rigidbody.position + direction * (_speed * Time.fixedDeltaTime)
            );

            Quaternion rightDirection = Quaternion.Euler(
                0f, _direction.x * (_turnSpeed * Time.fixedDeltaTime), 0f
            );
            Quaternion newRotation = Quaternion.Slerp(
                _rigidbody.rotation,
                _rigidbody.rotation * rightDirection,
                Time.fixedDeltaTime * 3f
            );
            _rigidbody.MoveRotation(newRotation);
        }
    }
}