using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : Entity
{

    [Header("References")]
    private Rigidbody2D rb;


    [Header("Player Movement")]
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float backSpeed;
    [SerializeField] private float forwardSpeed;
    private Vector2 currentJoystickPosition;
    private float deadzone = 0.3f;


    


    [Header("Player Shoot")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();        
    }

    protected override void FixedUpdate()
    {
        Movement();
        Jump();
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
    /// Handles the shoot logic
    /// </summary>
    private void Shoot()
    {
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            Projectile bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity).GetComponent<Projectile>();
            bullet.InitProjectile(Vector2.right, 10f, EntitySide.Player, GetHeight());
        }
    }

    #endregion

    #region Inputs

    /// <summary>
    /// Receives the player's movement input
    /// </summary>
    public void MovementInput(InputAction.CallbackContext context)
    {
        Vector2 inputVector = context.ReadValue<Vector2>();
        inputVector = new Vector2( Mathf.Abs(inputVector.x) > deadzone ? Mathf.Sign(inputVector.x) : 0, 
            Mathf.Abs(inputVector.y) > deadzone ? Mathf.Sign(inputVector.y) : 0);
        currentJoystickPosition = inputVector;
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

    /// <summary>
    /// Receives the player's shoot input
    /// </summary>
    public void ShootInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Shoot();
        }
    }

    #endregion
}
