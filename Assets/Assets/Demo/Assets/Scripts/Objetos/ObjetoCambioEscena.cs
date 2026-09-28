using UnityEngine;

/// <summary>
/// Cualquier objeto que, al hacer clic, cambie de escena (una flecha,
/// una puerta de salida, un portal, etc.) puede usar este mismo
/// componente. Cada instancia solo necesita indicar a qué escena
/// lleva desde el Inspector; no hace falta escribir un script nuevo
/// por cada flecha que agregues al juego.
///
/// Hereda de ObjetoSeleccionable para tener la misma animación de
/// hover que el resto de los objetos seleccionables.
/// </summary>
public class ObjetoCambioEscena : ObjetoSeleccionable
{
    [Tooltip("Debe coincidir EXACTAMENTE con el nombre de la escena. " +
             "Esa escena también debe estar agregada en File > Build Settings.")]
    public string nombreEscena;

    [Tooltip("Desmarcá esto si el fondo de destino calza justo con la " +
             "pantalla (sin margen de sobra): el acercamiento de cámara " +
             "revelaría un borde que no debería verse. El fundido a " +
             "negro se hace siempre, esto solo afecta el 'paso' previo.")]
    public bool usarEfectoCaminado = true;

    private void OnMouseDown()
    {
        GestorTransicionEscena.singleton.IrAEscena(nombreEscena, transform.position, usarEfectoCaminado);
    }

    private void OnMouseEnter()
    {
        IniciarHover();
    }

    private void OnMouseExit()
    {
        TerminarHover();
    }
}
