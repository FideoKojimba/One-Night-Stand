using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Único punto de control del inventario (patrón Singleton).
/// Es la clase "cerebro": decide EN QUÉ slot va cada ítem y
/// mantiene la lista real de lo que el jugador posee.
/// Las demás clases (Slot, Icon) solo ejecutan lo que este manager pide.
///
/// Persiste entre escenas (DontDestroyOnLoad), igual que
/// GestorTransicionEscena: si no persistiera, al cambiar de escena se
/// destruiría junto con todo lo que el jugador tenía en el inventario
/// (como la llave, antes de poder usarla).
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager singleton;

    [Header("Slots de la interfaz")]
    public InventorySlot[] slots;

    [Header("Prefab visual que se instancia dentro de cada slot")]
    public GameObject iconoItemPrefab;

    // Lista interna real de ítems (fuente de verdad del inventario)
    private List<Item> inventario = new List<Item>();

    // Nombres de ítems que el jugador ya recogió ALGUNA VEZ, sin importar
    // si siguen en el inventario o ya se consumieron. Sirve para que los
    // WorldItem no vuelvan a aparecer al recargar una escena ya visitada.
    private HashSet<string> nombresRecogidos = new HashSet<string>();

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Evita managers duplicados en la escena
        }
    }

    /// <summary>
    /// Busca el primer slot libre y coloca el ítem ahí.
    /// Devuelve true si se pudo agregar, false si el inventario está lleno.
    /// </summary>
    public bool AgregarItem(Item item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.EstaVacio)
            {
                InstanciarIcono(item, slot);
                inventario.Add(item);
                nombresRecogidos.Add(item.nombre);
                return true;
            }
        }

        Debug.Log("Inventario lleno: no hay slots disponibles");
        return false;
    }

    /// <summary>
    /// Indica si un ítem con este nombre ya fue recogido alguna vez,
    /// sin importar si sigue en el inventario o ya se usó/consumió.
    /// Cada WorldItem se consulta a sí mismo con esto al aparecer.
    /// </summary>
    public bool YaFueRecogido(string nombreItem)
    {
        return nombresRecogidos.Contains(nombreItem);
    }

    /// <summary>
    /// Quita el ítem de la lista, libera su slot y lo "suelta" en el
    /// mundo instanciando su prefab (igual que hacía SacarDelInventario).
    /// </summary>
    public void RemoverItem(Item item, InventorySlot slot)
    {
        inventario.Remove(item);
        slot.Liberar();

        if (item.prefab != null)
        {
            Instantiate(item.prefab, Vector3.zero, Quaternion.identity);
        }
    }

    /// <summary>
    /// Quita un ítem del inventario porque fue CONSUMIDO al usarlo sobre
    /// un objeto del mundo (por ejemplo, una llave que abrió una puerta).
    /// A diferencia de RemoverItem, esto NO instancia nada en el mundo:
    /// el ítem simplemente desaparece para siempre.
    /// </summary>
    public void ConsumirItem(Item item, InventorySlot slot)
    {
        inventario.Remove(item);
        slot.Liberar();
    }

    /// <summary>
    /// Vacía el inventario por completo: destruye los íconos que hay en
    /// pantalla, libera los slots y olvida qué ítems se recogieron alguna
    /// vez (así los WorldItem vuelven a aparecer al reiniciar el juego).
    /// </summary>
    public void Reiniciar()
    {
        foreach (InventorySlot slot in slots)
        {
            if (!slot.EstaVacio)
            {
                Destroy(slot.IconoActual.gameObject);
                slot.Liberar();
            }
        }

        inventario.Clear();
        nombresRecogidos.Clear();
    }

    private void InstanciarIcono(Item item, InventorySlot slot)
    {
        GameObject nuevoIcono = Instantiate(iconoItemPrefab, slot.transform);
        InventoryItemIcon icono = nuevoIcono.GetComponent<InventoryItemIcon>();
        icono.Inicializar(item, slot);
    }
}
