using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/*
 * Script de interacción para reproducir audio
 * - Usa el New Input System (InputActionReference)
 * - Requiere MANTENER presionado para llenar el círculo de progreso
 * - Solo reproduce el audio cuando el círculo llega al 100%
 * - Resetea el progreso si sueltas antes de completar
 * - El canvas solo aparece cuando el jugador entra al trigger
 */
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class AudioInteractor : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [Tooltip("Clip de audio a reproducir cuando se completa la interacción")]
    public AudioClip audioClip;

    [Tooltip("Si está marcado, el audio se reproduce una sola vez y luego el interactor se desactiva")]
    public bool playOnce = false;

    [Header("Referencias del Jugador")]
    [Tooltip("Arrastra el GameObject del jugador aquí")]
    public GameObject playerObject;

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
    private bool hasPlayedAudio = false;
    private AudioSource audioSource;
    private CanvasGroup canvasGroup;

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
        // Obtener el AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: No se encontró AudioSource");
        }

        ValidateReferences();
        ResetProgress();

        // Configurar CanvasGroup para controlar visibilidad
        if (interactionCanvas != null)
        {
            canvasGroup = interactionCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = interactionCanvas.AddComponent<CanvasGroup>();
                if (showDebug) Debug.Log("[AudioInteractor] CanvasGroup añadido automáticamente");
            }

            HideCanvas();

            if (showDebug)
            {
                Debug.Log($"[AudioInteractor] Canvas '{interactionCanvas.name}' inicializado y ocultado");
            }
        }
        else
        {
            Debug.LogError("[AudioInteractor] ¡interactionCanvas es NULL! Verifica el Inspector.");
        }
    }

    void Update()
    {
        // Si ya reprodujo el audio y es playOnce, no hacer nada
        if (playOnce && hasPlayedAudio)
            return;

        if (!isPlayerInRange)
            return;

        HandleInteraction();
    }

    #endregion

    #region Canvas Control

    private void HideCanvas()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void ShowCanvas()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
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
                if (showDebug) Debug.Log("[AudioInteractor] Comenzando interacción...");
            }

            // Incrementar progreso
            currentFillAmount += Time.deltaTime;
            float normalizedProgress = Mathf.Clamp01(currentFillAmount / fillTime);
            progressFillImage.fillAmount = normalizedProgress;

            if (showDebug && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[AudioInteractor] Progreso: {normalizedProgress * 100:F1}%");
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
            if (showDebug) Debug.Log("[AudioInteractor] Botón soltado - Reseteando progreso");
            ResetProgress();
        }
    }

    private void CompleteInteraction()
    {
        if (showDebug) Debug.Log("[AudioInteractor] ¡Interacción completada al 100%!");

        hasPlayedAudio = true;
        isInteracting = false;

        PlayAudio();

        // Si es playOnce, ocultar el canvas y desactivar interacciones
        if (playOnce)
        {
            HideCanvas();
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Disable();
            }
            if (showDebug) Debug.Log("[AudioInteractor] Modo PlayOnce - Interactor desactivado");
        }
        else
        {
            // Si no es playOnce, resetear para permitir otra interacción
            ResetProgress();
        }
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

    #region Audio Playback

    private void PlayAudio()
    {
        if (audioSource == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: AudioSource es NULL");
            return;
        }

        if (audioClip == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: No se asignó un AudioClip");
            return;
        }

        // Reproducir el audio
        audioSource.clip = audioClip;
        audioSource.Play();

        if (showDebug)
        {
            Debug.Log($"[AudioInteractor] Reproduciendo audio: {audioClip.name} (Duración: {audioClip.length:F2}s)");
        }
    }

    /// <summary>
    /// Método público para detener el audio (útil para otros scripts)
    /// </summary>
    public void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            if (showDebug) Debug.Log("[AudioInteractor] Audio detenido");
        }
    }

    /// <summary>
    /// Método público para pausar el audio
    /// </summary>
    public void PauseAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            if (showDebug) Debug.Log("[AudioInteractor] Audio pausado");
        }
    }

    /// <summary>
    /// Método público para reanudar el audio
    /// </summary>
    public void ResumeAudio()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
            if (showDebug) Debug.Log("[AudioInteractor] Audio reanudado");
        }
    }

    /// <summary>
    /// Método público para resetear el interactor (permite interactuar de nuevo si era playOnce)
    /// </summary>
    public void ResetInteractor()
    {
        hasPlayedAudio = false;
        ResetProgress();

        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.Enable();
        }

        if (showDebug) Debug.Log("[AudioInteractor] Interactor reseteado");
    }

    #endregion

    #region Trigger Detection

    private void OnTriggerEnter(Collider other)
    {
        if (showDebug)
        {
            Debug.Log($"[AudioInteractor] Trigger detectado: {other.gameObject.name}");
        }

        if (other.gameObject == playerObject)
        {
            // Si es playOnce y ya se reprodujo, no mostrar canvas
            if (playOnce && hasPlayedAudio)
            {
                if (showDebug) Debug.Log("[AudioInteractor] PlayOnce ya ejecutado - No se muestra canvas");
                return;
            }

            isPlayerInRange = true;

            // MOSTRAR el canvas cuando entra al trigger
            if (interactionCanvas != null)
            {
                ShowCanvas();

                if (showDebug)
                {
                    Debug.Log($"[AudioInteractor] ✅ Canvas VISIBLE - Alpha: {canvasGroup.alpha}");
                }
            }
            else
            {
                Debug.LogError("[AudioInteractor] ¡No se puede mostrar el canvas porque es NULL!");
            }

            ResetProgress();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerObject)
        {
            isPlayerInRange = false;

            // OCULTAR el canvas cuando sale del trigger
            if (interactionCanvas != null)
            {
                HideCanvas();

                if (showDebug)
                {
                    Debug.Log($"[AudioInteractor] ❌ Canvas OCULTO - Alpha: {canvasGroup.alpha}");
                }
            }

            ResetProgress();
        }
    }

    #endregion

    #region Validation

    private void ValidateReferences()
    {
        if (interactionCanvas == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: Falta asignar 'Interaction Canvas' (PickupPromptCanvas)");
            return;
        }

        // Verificar que tenga el componente Canvas
        Canvas canvas = interactionCanvas.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: El GameObject asignado no tiene un componente Canvas");
        }
        else if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
        {
            Debug.LogWarning($"[AudioInteractor] {gameObject.name}: Canvas en World Space sin Event Camera asignada. Asigna la Main Camera.");
        }

        if (progressFillImage == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: Falta asignar 'Progress Fill Image' (ProgressFill)");
        }
        else
        {
            if (progressFillImage.type != Image.Type.Filled)
            {
                Debug.LogWarning($"[AudioInteractor] {gameObject.name}: ProgressFillImage debe tener Image Type = Filled");
            }

            if (progressFillImage.fillMethod != Image.FillMethod.Radial360)
            {
                Debug.LogWarning($"[AudioInteractor] {gameObject.name}: ProgressFillImage debe tener Fill Method = Radial 360");
            }
        }

        if (playerObject == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: Falta asignar 'Player Object'");
        }

        if (interactAction == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: Falta asignar 'Interact Action' (Input Action Reference)");
        }
        else if (interactAction.action == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: La 'Interact Action' no tiene una acción válida asignada");
        }

        if (audioClip == null)
        {
            Debug.LogWarning($"[AudioInteractor] {gameObject.name}: No se asignó un AudioClip");
        }

        if (audioSource == null)
        {
            Debug.LogError($"[AudioInteractor] {gameObject.name}: No se encontró AudioSource. Asegúrate de tener el componente.");
        }
    }

    #endregion

    #region Gizmos (para debug visual)

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            // Color verde si el jugador está en rango, amarillo si no
            Gizmos.color = isPlayerInRange ? Color.green : Color.yellow;

            // Si ya reprodujo en modo PlayOnce, mostrar en rojo
            if (playOnce && hasPlayedAudio)
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawWireCube(transform.position + col.bounds.center, col.bounds.size);
        }
    }

    #endregion
}