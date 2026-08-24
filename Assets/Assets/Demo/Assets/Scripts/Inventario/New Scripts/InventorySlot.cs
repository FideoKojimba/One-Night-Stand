using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Representa UN espacio de la interfaz de inventario.
/// Sabe si está vacío u ocupado, y por quién.
/// También decide qué pasa cuando se suelta un ícono arrastrado sobre él
/// (IDropHandler): si está vacío lo acepta, si está ocupado intercambia
/// posiciones con el ícono que ya tenía. El slot decide el resultado del
/// drop; el ícono solo sabe moverse a sí mismo (responsabilidad única).
/// </summary>
public class InventorySlot : MonoBehaviour, IDropHandler
{
    // Ícono actualmente colocado en este slot (null = slot libre)
    private InventoryItemIcon iconoActual;

    public bool EstaVacio => iconoActual == null;
    public InventoryItemIcon IconoActual => iconoActual;

    public void AsignarIcono(InventoryItemIcon icono)
    {
        iconoActual = icono;
    }

    public void Liberar()
    {
        iconoActual = null;
    }

    /// <summary>
    /// Se llama automáticamente cuando el jugador suelta un ícono
    /// arrastrado justo encima de este slot.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        InventoryItemIcon iconoArrastrado = eventData.pointerDrag.GetComponent<InventoryItemIcon>();
        if (iconoArrastrado == null) return;

        InventorySlot slotOrigen = iconoArrastrado.SlotAsignado;
        if (slotOrigen == this) return; // soltado sobre el mismo slot: no hacer nada

        if (EstaVacio)
        {
            // Slot libre: el ícono simplemente se muda aquí
            iconoArrastrado.MoverA(this);
        }
        else
        {
            // Slot ocupado: intercambiar posiciones entre ambos íconos
            InventoryItemIcon iconoDestino = iconoActual;
            iconoDestino.MoverA(slotOrigen);
            iconoArrastrado.MoverA(this);
        }
    }
}
