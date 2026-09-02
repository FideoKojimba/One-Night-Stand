using UnityEngine;
using UnityEngine.SceneManagement;

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
public class FlechaCambioEscena : ObjetoSeleccionable
{
    [Tooltip("Debe coincidir EXACTAMENTE con el nombre de la escena. " +
             "Esa escena también debe estar agregada en File > Build Settings.")]
    public string nombreEscena;

    private void OnMouseDown()
    {
        SceneManager.LoadScene(nombreEscena);
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
