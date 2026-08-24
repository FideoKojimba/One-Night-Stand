using UnityEngine;

/// <summary>
/// Componente que va sobre un ítem físico en el mundo del juego.
/// Al hacer clic, le pide al InventoryManager que lo agregue.
/// Solo desaparece del mundo si realmente había espacio en el inventario
/// (antes, con ItemMundo, el objeto se destruía sin verificar eso).
/// </summary>
public class WorldItem : MonoBehaviour
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
}
