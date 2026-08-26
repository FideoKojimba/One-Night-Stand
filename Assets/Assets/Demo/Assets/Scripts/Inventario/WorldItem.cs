using UnityEngine;

/// <summary>
/// Componente que va sobre un ítem físico en el mundo del juego.
/// Al hacer clic, le pide al InventoryManager que lo agregue.
/// Solo desaparece del mundo si realmente había espacio en el inventario
/// (antes, con ItemMundo, el objeto se destruía sin verificar eso).
///
/// Hereda de ObjetoSeleccionable para obtener gratis la animación de
/// hover; solo necesita avisarle cuándo empieza/termina el hover,
/// usando los eventos físicos del mouse (funciona con Collider o
/// Collider2D, ya que este proyecto es 2D).
/// </summary>
public class WorldItem : ObjetoSeleccionable
{
    public Item item;

    private void OnMouseDown()
    {
        bool agregado = InventoryManager.singleton.AgregarItem(item);

        if (agregado)
        {
            Destroy(gameObject);
        }
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
