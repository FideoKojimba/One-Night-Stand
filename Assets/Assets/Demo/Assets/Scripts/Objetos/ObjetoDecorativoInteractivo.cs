using UnityEngine;

/// <summary>
/// Objeto que solo reacciona visualmente al pasar el mouse por encima
/// (hover), sin ninguna acción al hacer clic. Usa exactamente el mismo
/// mecanismo de detección que WorldItem y FlechaCambioEscena: hereda
/// de ObjetoSeleccionable y solo conecta los eventos de mouse físico
/// (OnMouseEnter/OnMouseExit) con IniciarHover()/TerminarHover().
/// </summary>
public class ObjetoDecorativoInteractivo : ObjetoSeleccionable
{
    private void OnMouseEnter()
    {
        IniciarHover();
    }

    private void OnMouseExit()
    {
        TerminarHover();
    }
}
