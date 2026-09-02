using UnityEngine;

/// <summary>
/// Ejemplo de objeto interactivo: una puerta que se abre si se le da
/// la llave correcta. Es solo UNA implementación posible de
/// IObjetoInteractivo; cualquier otro objeto (cofre, portón, jaula)
/// puede tener su propia clase con su propia lógica sin tocar el
/// sistema de inventario ni el de arrastre.
///
/// Al abrirse, la puerta desaparece por completo y revela otro objeto
/// en su lugar (por ejemplo, una flecha con FlechaCambioEscena). Ese
/// objeto de reemplazo es genérico a propósito: podría ser una flecha,
/// un cofre destapado, un pasadizo, lo que sea, sin tener que tocar
/// este script para cada puerta nueva que agregues.
/// </summary>
public class PuertaConLlave : MonoBehaviour, IObjetoInteractivo
{
    [Tooltip("Debe coincidir exactamente con el nombre del Item que la abre")]
    public string nombreLlaveRequerida = "Llave Dorada";

    [Header("Qué pasa al abrirse")]
    [Tooltip("El objeto de la puerta en sí: se desactiva por completo (sprite, collider, todo)")]
    public GameObject visualPuerta;

    [Tooltip("Lo que aparece en su lugar (una flecha, un pasadizo, etc.). Puede dejarse vacío si no aplica.")]
    public GameObject objetoQueAparece;

    public bool UsarItem(Item item)
    {
        Debug.Log("[DEBUG] Puerta recibió el ítem: '" + item.nombre + "' (esperaba: '" + nombreLlaveRequerida + "')");

        if (item.nombre != nombreLlaveRequerida)
        {
            return false; // No es la llave correcta: se rechaza
        }

        Abrir();
        return true; // Aceptado: la llave se consume
    }

    private void Abrir()
    {
        if (visualPuerta != null) visualPuerta.SetActive(false);
        if (objetoQueAparece != null) objetoQueAparece.SetActive(true);

        // Aquí también podrías reproducir un sonido, un efecto de partículas, etc.
    }
}
