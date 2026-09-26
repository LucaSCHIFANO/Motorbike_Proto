using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject visual;
    private Rigidbody2D rb;


    [Header("Player Movement")]
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float backSpeed;
    [SerializeField] private float forwardSpeed;
    private Vector2 currentJoystickPosition;


    [Header("Player Jump")]
    [SerializeField] private float jumpTime;
    [SerializeField] private float jumpForce;
    private float jumpVelocity;
    private bool isJumping = false;
    private float currentJumpTime = 0f;
    [SerializeField] private float fallMultiplier;
    private bool isGrounded = true;
    private Vector2 localVisualPosition;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();        
    }

    void Start()
    {
        localVisualPosition = visual.transform.localPosition;
    }

    void FixedUpdate()
    {
        Movement();
        Jump();
    }

    void Update()
    {
        // Manage jump time
        currentJumpTime -= Time.deltaTime;
        if (currentJumpTime <= 0)
        {
            isJumping = false;
        }

        if(visual.transform.localPosition.y < localVisualPosition.y)
        {
            visual.transform.localPosition = localVisualPosition;
            isGrounded = true;
            jumpVelocity = 0;
        }
    }

    #region Movements

    /// <summary>
    /// Handles the player's movement
    /// </summary>
    private void Movement() 
    {
        float horizontalSpeed = 0;
        if (currentJoystickPosition.x > 0)
        {
            horizontalSpeed = forwardSpeed;
        }
        else if (currentJoystickPosition.x < 0)
        {
            horizontalSpeed = backSpeed;
        }

        Vector2 movement = new Vector2(currentJoystickPosition.x * horizontalSpeed, currentJoystickPosition.y * verticalSpeed);
        rb.linearVelocity = movement;
    }

    /// <summary>
    /// Handles the player's jump
    /// </summary>
    private void Jump()
    {
        if (isJumping)
        {
            jumpVelocity = jumpForce * Time.deltaTime;
            isGrounded = false;
        }
        else if (!isGrounded)
        {
            jumpVelocity -= fallMultiplier * Time.fixedDeltaTime;
        }

        visual.transform.position += Vector3.up * jumpVelocity;
    }

    #endregion

    #region Inputs

    /// <summary>
    /// Receives the player's movement input
    /// </summary>
    public void MovementInput(InputAction.CallbackContext context)
    {
        currentJoystickPosition = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Receives the player's jump input
    /// </summary>
    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            isJumping = true;
            currentJumpTime = jumpTime;
        }
        else if (context.canceled) isJumping = false;
    }

    #endregion
}
