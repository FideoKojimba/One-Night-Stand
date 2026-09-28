using UnityEngine;

/// <summary>
/// Reinicia el juego completo: le pide a cada gestor persistente que
/// borre su propio estado (inventario y eventos ya ocurridos) y vuelve
/// a la escena inicial con el fundido a negro de siempre.
///
/// No borra nada por su cuenta: cada gestor sabe reiniciarse a sí mismo
/// (InventoryManager y GestorEstadoJuego), esta clase solo coordina.
/// Se conecta al OnClick de un botón, por ejemplo en la pantalla final.
/// </summary>
public class ReiniciadorDeJuego : MonoBehaviour
{
    [Tooltip("Nombre exacto de la primera escena del juego (debe estar en Build Settings)")]
    public string escenaInicial;

    private bool reiniciando = false;

    /// <summary>Conectado al botón de reiniciar desde el Inspector.</summary>
    public void Reiniciar()
    {
        // Evita disparar dos reinicios si el jugador hace doble clic.
        if (reiniciando)
        {
            return;
        }

        reiniciando = true;

        InventoryManager.singleton.Reiniciar();
        GestorEstadoJuego.singleton.Reiniciar();
        GestorTransicionEscena.singleton.IrAEscena(escenaInicial, transform.position, false);
    }
}
