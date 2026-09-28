using UnityEngine;

/// <summary>
/// Puerta que lleva a un puzzle (por ejemplo, un numpad) mientras ese
/// puzzle no se resolvió, y a otro destino distinto una vez que ya se
/// resolvió (el estado se consulta en GestorEstadoJuego, así que
/// persiste aunque recargues la escena).
///
/// Hereda de ObjetoSeleccionable para tener el mismo hover normal que
/// el resto de los objetos interactivos del juego.
/// </summary>
public class PuertaConCodigo : ObjetoSeleccionable
{
    [Tooltip("Identificador único del puzzle que desbloquea esta puerta " +
             "(el mismo id que usa el script Numpad correspondiente)")]
    public string idPuzzle = "PuzzleSinNombre";

    [Tooltip("Escena del puzzle (numpad), mientras no se haya resuelto todavía")]
    public string escenaPuzzle;

    [Tooltip("Escena a la que lleva esta puerta una vez que el puzzle YA se resolvió")]
    public string escenaDestinoFinal;

    [Tooltip("Si querés el efecto de caminado al ir hacia el destino final " +
             "(hacia el puzzle nunca se usa, ya que es solo una pantalla de numpad)")]
    public bool usarEfectoCaminado = true;

    private void OnMouseEnter()
    {
        IniciarHover();
    }

    private void OnMouseExit()
    {
        TerminarHover();
    }

    private void OnMouseDown()
    {
        if (GestorDialogo.EnDialogo) return;

        if (GestorEstadoJuego.singleton.YaOcurrio(idPuzzle))
        {
            GestorTransicionEscena.singleton.IrAEscena(escenaDestinoFinal, transform.position, usarEfectoCaminado);
        }
        else
        {
            GestorTransicionEscena.singleton.IrAEscena(escenaPuzzle, transform.position, false);
        }
    }
}
