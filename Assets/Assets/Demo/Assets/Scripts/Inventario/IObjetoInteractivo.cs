/// <summary>
/// Cualquier objeto del mundo que deba reaccionar a recibir un ítem
/// arrastrado desde el inventario implementa esta interfaz: una puerta,
/// un cofre, una fogata, un NPC, etc. El ícono del inventario no sabe
/// nada sobre estos objetos específicos; solo sabe que, si el objetivo
/// implementa esta interfaz, puede ofrecerle el ítem (polimorfismo:
/// mismo método, comportamiento distinto según la clase).
/// </summary>
public interface IObjetoInteractivo
{
    /// <summary>
    /// Se le ofrece un ítem a este objeto. Cada clase decide si lo
    /// acepta (revisando su nombre, un tag, lo que sea) y qué hacer
    /// si lo acepta (abrir una puerta, encender fuego, etc.).
    /// </summary>
    /// <returns>
    /// true si el objeto aceptó y "consumió" el ítem (debe desaparecer
    /// del inventario). false si lo rechazó (el ítem vuelve a su slot).
    /// </returns>
    bool UsarItem(Item item);
}
