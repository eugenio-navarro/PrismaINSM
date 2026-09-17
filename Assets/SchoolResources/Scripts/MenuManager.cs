using UnityEngine;
using UnityEngine.InputSystem; // ¡Necesario!
using UnityEngine.EventSystems; // ¡Necesario!
using UnityEngine.UI; // ¡Necesario para el botón!

// ESTA VERSIÓN NO REQUIERE EL COMPONENTE "PLAYER INPUT"
public class MenuManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el Panel que contiene tu menú")]
    public GameObject menuPanel;

    [Header("Navegación")]
    [Tooltip("El primer botón que debe seleccionarse al abrir el menú")]
    public Button firstSelectedButton; // Arrastra tu "Boton_Sala1"

    // En lugar de una referencia, creamos el objeto de controles manualmente
    private PlayerControls controls;

    private bool isMenuOpen = false;

    void Awake()
    {
        // 1. CREAMOS la instancia de los controles
        controls = new PlayerControls();

        // 2. Nos suscribimos a las acciones que nos interesan
        // (Asume que tus acciones se llaman "OpenMenu" y "CloseMenu" 
        // en tus mapas "Gameplay" y "UI")
        controls.Gameplay.OpenMenu.performed += OnOpenMenu;
        controls.UI.CloseMenu.performed += OnCloseMenu;

        // 3. Cerramos el menú al empezar
        menuPanel.SetActive(false);
        isMenuOpen = false;

        // Empezamos con los controles de Gameplay activos
        controls.Gameplay.Enable();
        controls.UI.Disable();
    }

    // ¡Importante! Debemos activar y desactivar los controles con el objeto
    private void OnEnable()
    {
        // Asegurarse de que los controles correctos estén activos
        // si volvemos de otra escena, etc.
        if (isMenuOpen)
        {
            controls.Gameplay.Disable();
            controls.UI.Enable();
        }
        else
        {
            controls.Gameplay.Enable();
            controls.UI.Disable();
        }
    }

    private void OnDisable()
    {
        // Desactiva todos los mapas para evitar errores cuando el objeto se destruye
        controls.Gameplay.Disable();
        controls.UI.Disable();
    }

    // Se llama cuando presionas "Tab" (desde el mapa Gameplay)
    private void OnOpenMenu(InputAction.CallbackContext context)
    {
        if (isMenuOpen) return;

        isMenuOpen = true;
        menuPanel.SetActive(true); // Muestra el menú
        Time.timeScale = 0f; // Pausa el juego

        // Cambia los mapas de acción MANUALMENTE
        controls.Gameplay.Disable();
        controls.UI.Enable();

        // Fija el foco en el primer botón
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }

    // Se llama cuando presionas "Tab" (desde el mapa UI)
    private void OnCloseMenu(InputAction.CallbackContext context)
    {
        if (!isMenuOpen) return;
        CloseMenu();
    }

    // Función pública para que otros scripts (o un botón "Atrás") la llamen
    public void CloseMenu()
    {
        isMenuOpen = false;
        menuPanel.SetActive(false); // Oculta el menú
        Time.timeScale = 1f; // Reanuda el juego

        // Cambia los mapas de acción MANUALMENTE
        controls.UI.Disable();
        controls.Gameplay.Enable();

        // Limpia la selección
        EventSystem.current.SetSelectedGameObject(null);
    }
}