using UnityEngine;

/// <summary>
/// Se coloca en un objeto de una escena donde NO debe verse la interfaz
/// de juego (por ejemplo, el menú principal): oculta la barra del
/// inventario y el ícono de pausa. Al entrar a la escena los oculta, y
/// al salir los vuelve a mostrar, para que el resto de escenas no
/// tengan que acordarse de nada.
///
/// No sabe cómo se ocultan: solo se lo pide a InventoryManager y a
/// GestorPausa (responsabilidad única).
/// </summary>
public class OcultarInventarioEnEscena : MonoBehaviour
{
    private void Start()
    {
        InventoryManager.singleton.MostrarInterfaz(false);

        if (GestorPausa.singleton != null)
        {
            GestorPausa.singleton.MostrarIcono(false);
        }
    }

    // Se ejecuta cuando la escena se descarga (o se detiene el juego)
    private void OnDestroy()
    {
        if (InventoryManager.singleton != null)
        {
            InventoryManager.singleton.MostrarInterfaz(true);
        }

        if (GestorPausa.singleton != null)
        {
            GestorPausa.singleton.MostrarIcono(true);
        }
    }
}
