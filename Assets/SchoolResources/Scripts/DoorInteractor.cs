using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/*
 * Script de interacción con puerta/placeholder mejorado
 * - Usa el New Input System (InputActionReference)
 * - Requiere MANTENER presionado para llenar el círculo de progreso
 * - Solo teletransporta cuando el círculo llega al 100%
 * - Resetea el progreso si sueltas antes de completar
 * - El canvas solo aparece cuando el jugador (detectado por Tag) entra al trigger
 */
[RequireComponent(typeof(Collider))]
public class DoorInteractor : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre exacto de la escena a cargar")]
    public string sceneToLoad;

    [Header("Referencias del Jugador")]
    // --- CAMBIO: 'playerObject' ELIMINADO ---
    
    [Tooltip("Arrastra la Input Action Reference para interactuar")]
    public InputActionReference interactAction;

    [Header("Referencias de UI")]
    [Tooltip("El Canvas principal (PickupPromptCanvas)")]
    public GameObject interactionCanvas;

    [Tooltip("La imagen ProgressFill con Fill Method: Radial 360")]
    public Image progressFillImage;

    [Header("Configuración de Progreso")]
    [Tooltip("Tiempo en segundos para completar el círculo")]
    [Range(0.5f, 5f)]
    public float fillTime = 1.5f;

    [Tooltip("Mostrar debug en consola")]
    public bool showDebug = false;

    // Variables privadas
    private bool isPlayerInRange = false;
    private bool isInteracting = false;
    private float currentFillAmount = 0f;
    private bool hasLoadedScene = false; // Prevenir carga múltiple

    #region Unity Lifecycle

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.Disable();
        }
    }

    void Start()
    {
        ValidateReferences();
        ResetProgress();

        // IMPORTANTE: Ocultar canvas al inicio
        if (interactionCanvas != null)
        {
            interactionCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (!isPlayerInRange || hasLoadedScene)
            return;

        HandleInteraction();
    }

    #endregion

    #region Interaction Logic

    private void HandleInteraction()
    {
        // Verificar que la acción esté asignada
        if (interactAction == null || interactAction.action == null)
            return;

        // El jugador está MANTENIENDO presionado el botón
        if (interactAction.action.IsPressed())
        {
            if (!isInteracting)
            {
                isInteracting = true;
                if (showDebug) Debug.Log("[DoorInteractor] Comenzando interacción...");
            }

            // Incrementar progreso
            currentFillAmount += Time.deltaTime;
            float normalizedProgress = Mathf.Clamp01(currentFillAmount / fillTime);
            progressFillImage.fillAmount = normalizedProgress;

            if (showDebug && Time.frameCount % 30 == 0) // Log cada medio segundo aprox
            {
                Debug.Log($"[DoorInteractor] Progreso: {normalizedProgress * 100:F1}%");
            }

            // Comprobar si se completó al 100%
            if (currentFillAmount >= fillTime)
            {
                CompleteInteraction();
            }
        }
        // El jugador SOLTÓ el botón
        else if (isInteracting)
        {
            // Resetear si no completó
            if (showDebug) Debug.Log("[DoorInteractor] Botón soltado - Reseteando progreso");
            ResetProgress();
        }
    }

    private void CompleteInteraction()
    {
        if (showDebug) Debug.Log("[DoorInteractor] ¡Interacción completada al 100%!");

        hasLoadedScene = true;
        isInteracting = false;

        // Desactivar input para prevenir inputs adicionales
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.Disable();
        }

        LoadTargetScene();
    }

    private void ResetProgress()
    {
        isInteracting = false;
        currentFillAmount = 0f;

        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = 0f;
        }
    }

    #endregion

    #region Scene Loading

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError($"[DoorInteractor] {gameObject.name}: No se especificó una escena en 'sceneToLoad'");
            return;
        }

        if (showDebug) Debug.Log($"[DoorInteractor] Cargando escena: {sceneToLoad}");

        SceneManager.LoadScene(sceneToLoad);
    }

    #endregion

    #region Trigger Detection

    private void OnTriggerEnter(Collider other)
    {
        // --- CAMBIO: Detectar al jugador por Tag ---
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // MOSTRAR el canvas cuando entra al trigger
            if (interactionCanvas != null)
            {
                interactionCanvas.SetActive(true);
            }

            ResetProgress();

            if (showDebug) Debug.Log("[DoorInteractor] Jugador entró en rango - Canvas visible");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // --- CAMBIO: Detectar al jugador por Tag ---
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // OCULTAR el canvas cuando sale del trigger
            if (interactionCanvas != null)
            {
                interactionCanvas.SetActive(false);
            }

            ResetProgress();

            if (showDebug) Debug.Log("[DoorInteractor] Jugador salió del rango - Canvas oculto");
        }
    }

    #endregion

    #region Validation

    private void ValidateReferences()
    {
        if (interactionCanvas == null)
        {
            Debug.LogError($"[DoorInteractor] {gameObject.name}: Falta asignar 'Interaction Canvas' (PickupPromptCanvas)");
        }

        if (progressFillImage == null)
        {
            Debug.LogError($"[DoorInteractor] {gameObject.name}: Falta asignar 'Progress Fill Image' (ProgressFill)");
        }
        else
        {
            // Verificar que tenga Image Type: Filled
            if (progressFillImage.type != Image.Type.Filled)
            {
                Debug.LogWarning($"[DoorInteractor] {gameObject.name}: ProgressFillImage debe tener Image Type = Filled");
            }

            // Verificar configuración Radial 360
            if (progressFillImage.fillMethod != Image.FillMethod.Radial360)
            {
                Debug.LogWarning($"[DoorInteractor] {gameObject.name}: ProgressFillImage debe tener Fill Method = Radial 360");
            }
        }

        // --- CAMBIO: Validación de 'playerObject' ELIMINADA ---

        if (interactAction == null)
        {
            Debug.LogError($"[DoorInteractor] {gameObject.name}: Falta asignar 'Interact Action' (Input Action Reference)");
        }
        else if (interactAction.action == null)
        {
            Debug.LogError($"[DoorInteractor] {gameObject.name}: La 'Interact Action' no tiene una acción válida asignada");
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning($"[DoorInteractor] {gameObject.name}: No se especificó una escena en 'sceneToLoad'");
        }
    }

    #endregion

    #region Gizmos (para debug visual)

    private void OnDrawGizmos()
    {
        // Dibujar el área de trigger en el editor
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Gizmos.color = isPlayerInRange ? Color.green : Color.yellow;
            // Corregido para dibujar en la posición correcta del collider
            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphereCol.center, sphereCol.radius);
            }
            else
            {
                // Fallback para otros colliders, aunque puede no ser preciso
                Gizmos.DrawWireMesh(GetComponent<MeshFilter>().sharedMesh, 0, transform.position, transform.rotation, transform.lossyScale);
            }
        }
    }

    #endregion
}