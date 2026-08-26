using UnityEngine;

/// <summary>
/// Ejemplo de objeto interactivo: una puerta que se abre si se le da
/// la llave correcta. Es solo UNA implementación posible de
/// IObjetoInteractivo; cualquier otro objeto (cofre, portón, jaula)
/// puede tener su propia clase con su propia lógica sin tocar el
/// sistema de inventario ni el de arrastre.
/// </summary>
public class PuertaConLlave : MonoBehaviour, IObjetoInteractivo
{
    [Tooltip("Debe coincidir exactamente con el nombre del Item que la abre")]
    public string nombreLlaveRequerida = "Llave Dorada";

    [Header("Referencias opcionales para el efecto visual")]
    public GameObject visualCerrada;
    public GameObject visualAbierta;

    public bool UsarItem(Item item)
    {
        if (item.nombre != nombreLlaveRequerida)
        {
            return false; // No es la llave correcta: se rechaza
        }

        AbrirPuerta();
        return true; // Aceptado: la llave se consume
    }

    private void AbrirPuerta()
    {
        if (visualCerrada != null) visualCerrada.SetActive(false);
        if (visualAbierta != null) visualAbierta.SetActive(true);
       Destroy(gameObject); // Destruye la puerta para que el jugador pueda pasar
        // Aquí también podrías reproducir un sonido, activar un
        // Animator, desactivar un Collider de bloqueo, etc.
    }
}
