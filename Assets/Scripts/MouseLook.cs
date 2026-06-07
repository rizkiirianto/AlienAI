using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MouseLook : MonoBehaviour
{
    [Header("Base Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    private float xRotation = 0f;

    [Header("Head Bobbing")]
    public float walkBobSpeed = 14f;
    public float sprintBobSpeed = 18f;
    public float bobAmount = 0.05f;
    private float defaultPosY;
    private float timer = 0f;

    [Header("Camera Tilt (Leaning)")]
    public float maxTiltAngle = 2f;
    public float tiltSpeed = 5f;
    private float currentTilt = 0f;

    [Header("Dynamic FOV")]
    public float baseFOV = 60f;
    public float sprintFOV = 75f;
    public float fovSpeed = 8f;
    private Camera cam;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        cam = GetComponent<Camera>();
        cam.fieldOfView = baseFOV;
        defaultPosY = transform.localPosition.y;
    }

    void Update()
    {
        HandleMouseLook();
        HandleCameraEffects();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply Left/Right rotation to the player body
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleCameraEffects()
    {
        // Read player movement input to trigger effects
        float moveX = Input.GetAxis("Horizontal"); // A/D keys
        float moveZ = Input.GetAxis("Vertical");   // W/S keys
        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && moveZ > 0;

        // 1. DYNAMIC FOV
        float targetFOV = isSprinting ? sprintFOV : baseFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSpeed);

        // 2. CAMERA TILT (Leaning)
        // If pressing A (left), tilt right. If pressing D (right), tilt left.
        float targetTilt = -moveX * maxTiltAngle; 
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Apply up/down look AND the Z-axis tilt
        transform.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);

        // 3. HEAD BOBBING
        if (isMoving)
        {
            // Increase timer based on walking or sprinting speed
            timer += Time.deltaTime * (isSprinting ? sprintBobSpeed : walkBobSpeed);
            
            // Apply Sine wave to Y position
            float newY = defaultPosY + Mathf.Sin(timer) * bobAmount;
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
        else
        {
            // Smoothly reset camera to default height when standing still
            timer = 0f;
            float smoothY = Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkBobSpeed);
            transform.localPosition = new Vector3(transform.localPosition.x, smoothY, transform.localPosition.z);
        }
    }
}