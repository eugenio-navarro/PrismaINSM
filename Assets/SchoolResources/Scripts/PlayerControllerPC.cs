using UnityEngine;
using UnityEngine.InputSystem;

// Esta es la versión para PC / WebGL.
// El único cambio es que la lógica del ratón y teclado
// ahora se incluye en WebGL, no solo en el Editor.

[RequireComponent(typeof(CharacterController))]
public class PCPlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cameraRoot; // Asegúrate que esta sea la cámara de PC

    [Header("Movimiento")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.5f;
    public float jumpHeight = 1.4f;
    public float gravity = -9.81f;

    // CAMBIO 1: Esta sección ahora se compila para Editor Y WebGL
#if UNITY_EDITOR || UNITY_WEBGL
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

        // CAMBIO 2: Bloqueamos el cursor en PC/WebGL
#if UNITY_EDITOR || UNITY_WEBGL
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    private void OnDisable()
    {
        if (moveAction.action != null) moveAction.action.Disable();
        if (jumpAction.action != null) jumpAction.action.Disable();
        if (sprintAction.action != null) sprintAction.action.Disable();

        // CAMBIO 3: Desbloqueamos el cursor en PC/WebGL
#if UNITY_EDITOR || UNITY_WEBGL
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#endif
    }
    #endregion

    private void Update()
    {
        // CAMBIO 4: La función de ratón ahora se llama en Editor Y WebGL
#if UNITY_EDITOR || UNITY_WEBGL
        HandleMouseLook();
#endif
        HandleMoveAndGravity();
    }

    // CAMBIO 5: Esta función ahora se compila para Editor Y WebGL
#if UNITY_EDITOR || UNITY_WEBGL
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

        // CAMBIO 6: Los inputs de teclado ahora se compilan para Editor Y WebGL
#if UNITY_EDITOR || UNITY_WEBGL
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