using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 5f;
    public float walkSpeed = 2.5f;
    public float crouchSpeed = 1.8f;
    public float jumpForce = 5f;
    public float gravity = -15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform cameraHolder;

    [Header("State")]
    public bool isGrounded;
    public bool isCrouching;
    public bool isWalking;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float normalHeight = 2f;
    private float crouchHeight = 1.2f;
    private Mouse mouse;
    private Keyboard keyboard;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Don't lock cursor if menu is showing
        if (!MainMenu.showMenu)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        normalHeight = controller.height;

        // Normal eye height
        if (cameraHolder != null)
            cameraHolder.localPosition = new Vector3(0, 0.6f, 0);
        mouse = Mouse.current;
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (mouse == null) mouse = Mouse.current;
        if (keyboard == null) keyboard = Keyboard.current;
        if (mouse == null || keyboard == null) return;

        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Mouse look
        Vector2 mouseDelta = mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.1f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.1f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70f, 75f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Walk / Crouch (CS2 style: default=run, shift=walk)
        isWalking = keyboard.leftShiftKey.isPressed;
        isCrouching = keyboard.leftCtrlKey.isPressed;

        float speed = runSpeed;
        if (isWalking) speed = walkSpeed;
        if (isCrouching) speed = crouchSpeed;

        // Crouch height
        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);

        // Movement
        float x = 0f, z = 0f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.magnitude > 1f) move.Normalize();
        controller.Move(move * speed * Time.deltaTime);

        // Jump
        if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
            velocity.y = jumpForce;

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Unlock cursor with ESC
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }
}
