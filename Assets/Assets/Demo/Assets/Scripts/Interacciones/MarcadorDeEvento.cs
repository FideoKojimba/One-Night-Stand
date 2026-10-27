using UnityEngine;

/// <summary>
/// Mini evento reutilizable: deja anotado en GestorEstadoJuego que algo
/// ocurrió (por ejemplo, "el jugador recogió el bisturí"), para que OTRAS
/// escenas puedan reaccionar con MostrarSiOcurrioEvento usando ese mismo id.
///
/// No decide nada ni cambia nada por sí solo: solo anota. Se conecta desde
/// el Inspector a cualquier UnityEvent (WorldItem > Al Recogerse,
/// DialogoTrigger > Al Terminar Dialogo, WorldNota > Al Cerrar Nota...)
/// eligiendo MarcadorDeEvento > Marcar().
///
/// Va sobre un objeto que SIEMPRE esté activo y que no se destruya al
/// usarlo (no lo pongas sobre el propio WorldItem, que se destruye).
/// </summary>
public class MarcadorDeEvento : MonoBehaviour
{
    [Tooltip("Identificador ÚNICO del evento que se anota (ej. 'BisturiRecogido'). " +
             "Debe ser idéntico al que se consulta en MostrarSiOcurrioEvento.")]
    [SerializeField] private string idEvento = "";

    /// <summary>Anota el evento como ocurrido (para siempre).</summary>
    public void Marcar()
    {
        if (string.IsNullOrEmpty(idEvento)) return;

        GestorEstadoJuego.singleton.MarcarComoOcurrido(idEvento);
    }
}
