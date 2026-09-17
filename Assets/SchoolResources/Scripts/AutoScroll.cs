using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Coloca este script en el objeto "Scroll View"
[RequireComponent(typeof(ScrollRect))]
public class AutoScroll : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;
    private GridLayoutGroup gridLayout; // Referencia para leer el padding

    private RectTransform currentSelected;

    // --- ¡¡¡NUEVA FUNCIÓN!!! ---
    // Esta función se llama automáticamente cuando este objeto
    // (el Scroll View) se desactiva (ej. cuando se cierra el panel).
    void OnDisable()
    {
        // Forzamos el scroll a volver a su posición inicial (arriba).
        // 1f = 100% (arriba), 0.5f = 50% (medio), 0f = 0% (abajo).
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
    // --- FIN DE LA NUEVA FUNCIÓN ---

    void Awake()
    {
        // Obtenemos las referencias automáticamente
        scrollRect = GetComponent<ScrollRect>();
        viewport = scrollRect.viewport;
        content = scrollRect.content;

        // Obtenemos el Grid Layout Group del Content
        if (content != null)
        {
            gridLayout = content.GetComponent<GridLayoutGroup>();
        }
    }

    // ¡¡¡CAMBIO CLAVE!!! Usamos LateUpdate() en lugar de Update()
    // LateUpdate() se ejecuta DESPUÉS de que toda la UI (Layouts, etc.)
    // ha terminado de calcularse. Esto evita la "pelea" y el "jitter".
    void LateUpdate()
    {
        // Obtener el objeto actualmente seleccionado por el EventSystem
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        // Salir si no hay nada seleccionado o si no es un hijo de nuestro "Content"
        if (selected == null || !selected.transform.IsChildOf(content))
        {
            currentSelected = null;
            return;
        }

        // Si es un nuevo objeto seleccionado
        currentSelected = selected.GetComponent<RectTransform>();

        // --- Cálculos de posición ---
        // (Usamos los valores ya calculados y estables del frame)
        float buttonTop = -currentSelected.anchoredPosition.y - (currentSelected.rect.height / 2f);
        float buttonBottom = -currentSelected.anchoredPosition.y + (currentSelected.rect.height / 2f);

        float viewportTop = content.anchoredPosition.y;
        float viewportBottom = content.anchoredPosition.y + viewport.rect.height;

        // --- El Cálculo Mágico (LA CORRECCIÓN) ---

        // Leemos el padding. Si no hay grid, el padding es 0.
        float paddingTop = 0;
        float paddingBottom = 0;

        if (gridLayout != null)
        {
            // Como quitaste los paddings, esto dará 0, lo cual es correcto.
            // Si los vuelves a poner, funcionará igual.
            paddingTop = gridLayout.padding.top;
            paddingBottom = gridLayout.padding.bottom;
        }

        // Si el botón está ARRIBA del viewport (necesitamos bajar el content)
        if (buttonTop < viewportTop)
        {
            // ¡CORREGIDO! Restamos el padding.top del cálculo.
            float newY = buttonTop - paddingTop;
            SetContentY(newY);
        }
        // Si el botón está DEBAJO del viewport (necesitamos subir el content)
        else if (buttonBottom > viewportBottom)
        {
            // ¡CORREGIDO! Sumamos el padding.bottom al cálculo.
            float newY = buttonBottom - viewport.rect.height + paddingBottom;
            SetContentY(newY);
        }
    }

    private void SetContentY(float newY)
    {
        // ¡SEGURIDAD! Asegura que la posición Y nunca sea menor que 0.
        // Esto detiene el "jitter" o pelea en el borde superior.
        Vector2 newPos = new Vector2(content.anchoredPosition.x, Mathf.Max(0f, newY));
        content.anchoredPosition = newPos;
    }
}