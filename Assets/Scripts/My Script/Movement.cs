using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    
    [Header("Jumping & Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -19.62f; // Double standard gravity feels less "floaty"

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        // Automatically grab the Character Controller component
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. GROUND CHECK
        // Creates a tiny invisible sphere at the player's feet to check for the floor
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            // Keep the player snapped to the ground smoothly
            velocity.y = -2f; 
        }

        // 2. READ MOVEMENT INPUT
        float x = Input.GetAxis("Horizontal"); // A/D keys
        float z = Input.GetAxis("Vertical");   // W/S keys
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        // 3. MOVE ALONG THE X AND Z AXES
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        // This calculates movement relative to where the player is currently looking
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 4. JUMPING
        // Uses standard physics formula for jump height
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 5. APPLY GRAVITY (Y AXIS)
        velocity.y += gravity * Time.deltaTime;
        
        // Apply the gravity/jump velocity to the controller
        controller.Move(velocity * Time.deltaTime);
    }
}