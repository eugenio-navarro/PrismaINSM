using UnityEngine;
using UnityEngine.Events;

public class InteractiveObject : MonoBehaviour
{
    // Puedes usar estos eventos en el Inspector para hacer que ocurran cosas
    // sin necesidad de modificar el código.
    public UnityEvent OnObjectEnter;
    public UnityEvent OnObjectExit;
    public UnityEvent OnObjectClick;

    /// <summary>
    /// Esta función es llamada por el script CardboardReticlePointer cuando
    /// la retícula empieza a apuntar a este objeto.
    /// </summary>
    public void OnPointerEnter()
    {
        // Aquí puedes poner código que se ejecute cuando miras el objeto.
        // Por ejemplo, cambiar su color o tamaño.

        // Ejecuta cualquier evento que hayas asignado en el Inspector.
        OnObjectEnter.Invoke();
    }

    /// <summary>
    /// Esta función es llamada por el script CardboardReticlePointer cuando
    /// la retícula deja de apuntar a este objeto.
    /// </summary>
    public void OnPointerExit()
    {
        // Aquí puedes poner código para revertir los cambios de OnPointerEnter.
        // Por ejemplo, devolver el objeto a su color o tamaño original.

        // Ejecuta cualquier evento que hayas asignado en el Inspector.
        OnObjectExit.Invoke();
    }

    /// <summary>
    /// Esta función es llamada por el script CardboardReticlePointer cuando
    /// se presiona el gatillo mientras se apunta a este objeto.
    /// </summary>
    public void OnPointerClick()
    {
        // Aquí puedes poner el código de la acción principal del objeto.
        // Por ejemplo, abrir una puerta, recoger un objeto, etc.

        // Ejecuta cualquier evento que hayas asignado en el Inspector.
        OnObjectClick.Invoke();
    }
}