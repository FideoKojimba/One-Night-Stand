using UnityEngine;

/// <summary>
/// Opción de menú que inicia el juego: lleva a la primera escena usando
/// el mismo fundido a negro que el resto del juego (GestorTransicionEscena).
/// Solo define QUÉ hace al confirmarse; el hover y los sonidos los hereda
/// de OpcionMenu.
/// </summary>
public class OpcionComenzarJuego : OpcionMenu
{
    [Header("Destino")]
    [Tooltip("Nombre EXACTO de la primera escena del juego (debe estar en Build Settings)")]
    [SerializeField] private string escenaInicial;

    protected override void Ejecutar()
    {
        // false = sin efecto de caminado, solo el fundido a negro
        GestorTransicionEscena.singleton.IrAEscena(escenaInicial, transform.position, false);
    }
}
