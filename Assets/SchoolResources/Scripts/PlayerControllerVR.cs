using UnityEngine;
using UnityEngine.InputSystem;

// Este es tu script original, ideal para el jugador de VR.
// La rotación de la cámara (cameraRoot) debe ser manejada
// por el sistema de VR (ej. TrackedPoseDriver).

[RequireComponent(typeof(CharacterController))]
public class VRPlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cameraRoot;

    [Header("Movimiento")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float jumpHeight = 1.4f;
    public float gravity = -9.81f;

#if UNITY_EDITOR
    [Header("Editor Testing (Mouse Look)")]
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;
#endif

    [Header("Input (Input System)")]
    public InputActionProperty moveAction;
    public InputActionProperty jumpAction;
    public InputActionProperty sprintAction;

    private Vector3 velocity;

    #region Standard Methods
    private void Reset()
    {
        controller = GetComponent<CharacterController>();
        if (cameraRoot == null && Camera.main != null)
            cameraRoot = Camera.main.transform;
    }

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (cameraRoot == null && Camera.main != null)
            cameraRoot = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (moveAction.action != null) moveAction.action.Enable();
        if (jumpAction.action != null) jumpAction.action.Enable();
        if (sprintAction.action != null) sprintAction.action.Enable();

        // No bloqueamos el cursor en VR
    }

    private void OnDisable()
    {
        if (moveAction.action != null) moveAction.action.Disable();
        if (jumpAction.action != null) jumpAction.action.Disable();
        if (sprintAction.action != null) sprintAction.action.Disable();
    }
    #endregion

    private void Update()
    {
#if UNITY_EDITOR
        HandleMouseLook();
#endif
        HandleMoveAndGravity();
    }

#if UNITY_EDITOR
    // Esta función SÓLO existe en el Editor, para pruebas.
    private void HandleMouseLook()
    {
        if (cameraRoot == null || Mouse.current == null) return;
        float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
        float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
#endif

    private void HandleMoveAndGravity()
    {
        if (controller == null || cameraRoot == null) return;

        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 1. CÁLCULO DEL MOVIMIENTO HORIZONTAL
        Vector2 move2D = moveAction.action != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        Vector3 forward = cameraRoot.forward;
        Vector3 right = cameraRoot.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Vector3 moveDirection = (right * move2D.x + forward * move2D.y);

        bool isSprinting = sprintAction.action != null && sprintAction.action.IsPressed();
        bool jumpPressed = jumpAction.action != null && jumpAction.action.WasPressedThisFrame();

#if UNITY_EDITOR
        // Los inputs de teclado SÓLO existen en el Editor, para pruebas.
        if (Keyboard.current != null)
        {
            isSprinting = isSprinting || Keyboard.current.leftShiftKey.isPressed;
            jumpPressed = jumpPressed || Keyboard.current.spaceKey.wasPressedThisFrame;
        }
#endif
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        velocity.x = moveDirection.x * currentSpeed;
        velocity.z = moveDirection.z * currentSpeed;

        // 2. CÁLCULO DEL SALTO
        if (grounded && jumpPressed)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 3. APLICACIÓN DE LA GRAVEDAD
        velocity.y += gravity * Time.deltaTime;

        // 4. MOVIMIENTO FINAL
        controller.Move(velocity * Time.deltaTime);
    }
}