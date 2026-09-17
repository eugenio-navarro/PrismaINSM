using UnityEngine;

// Este script decide qué jugador activar
// dependiendo de la plataforma.
// MODIFICADO: Ahora el Editor de Unity SIEMPRE activa el PlayerPC por defecto.
public class PlatformLoader : MonoBehaviour
{
    [Header("Objetos de Jugador")]
    [Tooltip("Arrastra aquí tu Prefab/GameObject del jugador de VR")]
    public GameObject playerVR;

    [Tooltip("Arrastra aquí tu Prefab/GameObject del jugador de PC/WebGL")]
    public GameObject playerPC;

    // --- Variable 'testPCInEditor' eliminada para mayor simplicidad ---

    void Awake()
    {
        // Esta es la "Compilación Dependiente de Plataforma"

#if UNITY_WEBGL
        // --- 1. ESTAMOS EN UN BUILD DE WEBGL ---
        // Plataforma final, siempre activar PC.
        Debug.Log("PLATAFORMA: WebGL Build. Activando jugador de PC.");
        ActivatePC();

#elif UNITY_EDITOR
        // --- 2. ESTAMOS EN EL EDITOR DE UNITY ---
        // Como solicitaste, SIEMPRE activamos el PlayerPC para pruebas.
        Debug.Log("PLATAFORMA: Editor. Activando jugador de PC para pruebas.");
        ActivatePC();

#else
        // --- 3. ESTAMOS EN OTRA PLATAFORMA (ANDROID/OCULUS, ETC.) ---
        // Plataforma final, activar VR.
        Debug.Log("PLATAFORMA: Android/Standalone. Activando jugador de VR.");
        ActivateVR();
#endif
    }

    // --- Funciones de ayuda para no repetir código ---

    void ActivatePC()
    {
        if (playerPC != null)
            playerPC.SetActive(true);
        if (playerVR != null)
            playerVR.SetActive(false);
    }

    void ActivateVR()
    {
        if (playerVR != null)
            playerVR.SetActive(true);
        if (playerPC != null)
            playerPC.SetActive(false);
    }
}