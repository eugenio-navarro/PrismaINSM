using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Esta función será llamada por los botones
    public void LoadSceneByName(string sceneName)
    {
        // ¡MUY IMPORTANTE! Reanuda el juego antes de cargar la escena
        Time.timeScale = 1f;

        // Carga la escena por su nombre
        SceneManager.LoadScene(sceneName);
    }
}