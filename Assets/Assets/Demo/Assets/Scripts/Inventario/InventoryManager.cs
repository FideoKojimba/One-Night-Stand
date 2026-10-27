using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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

    [Header("Interfaz")]
    [Tooltip("El objeto InventoryPanel (la barra con los slots). Se usa para " +
             "mostrarlo u ocultarlo, por ejemplo en los menús.")]
    public GameObject panelInventario;

    // Lista interna real de ítems (fuente de verdad del inventario)
    private List<Item> inventario = new List<Item>();

    // Qué ítem ocupa cada slot. Permite encontrar y quitar un ítem por su
    // nombre (DescartarItem) sin depender de los detalles internos del ícono.
    private Dictionary<InventorySlot, Item> itemPorSlot = new Dictionary<InventorySlot, Item>();

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
    /// Muestra u oculta SOLO la barra visual del inventario. Los ítems no
    /// se pierden: siguen guardados en la lista interna y en sus slots,
    /// simplemente no se ven (útil para menús, pausa o cinemáticas).
    /// </summary>
    public void MostrarInterfaz(bool visible)
    {
        if (panelInventario != null)
        {
            panelInventario.SetActive(visible);
        }
    }

    /// <summary>
    /// Busca el primer slot libre y coloca el ítem ahí.
    /// Devuelve true si se pudo agregar, false si el inventario está lleno.
    /// </summary>
    /// <summary>
    /// True si el puntero del mouse está justo encima del inventario (un slot o un
    /// ícono). Los objetos del mundo lo consultan en su OnMouseDown: OnMouseDown
    /// ignora la interfaz y "atraviesa" el inventario, así que sin esta revisión
    /// un clic sobre un ícono también agarraba el objeto que hubiera detrás.
    /// </summary>
    public static bool PunteroSobreInventario()
    {
        if (singleton == null || singleton.panelInventario == null) return false;
        if (!singleton.panelInventario.activeInHierarchy || EventSystem.current == null) return false;

        PointerEventData datos = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> resultados = new List<RaycastResult>();
        EventSystem.current.RaycastAll(datos, resultados);

        Transform panel = singleton.panelInventario.transform;
        foreach (RaycastResult r in resultados)
        {
            if (r.gameObject.transform.IsChildOf(panel)) return true;
        }
        return false;
    }

    public bool AgregarItem(Item item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.EstaVacio)
            {
                InstanciarIcono(item, slot);
                inventario.Add(item);
                itemPorSlot[slot] = item;
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
        itemPorSlot.Remove(slot);
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
        itemPorSlot.Remove(slot);
        slot.Liberar();
    }

    /// <summary>Indica si el jugador tiene ahora mismo un ítem con este nombre en el inventario.</summary>
    public bool TieneItem(string nombreItem)
    {
        foreach (Item item in itemPorSlot.Values)
        {
            if (item.nombre == nombreItem) return true;
        }
        return false;
    }

    /// <summary>
    /// Quita del inventario, sin dejar rastro, el ítem con este nombre
    /// (por ejemplo, un gadget que ya no sirve al terminar un puzzle).
    /// Devuelve true si lo encontró y lo quitó, false si no lo tenía.
    /// </summary>
    public bool DescartarItem(string nombreItem)
    {
        InventorySlot slotEncontrado = null;

        foreach (KeyValuePair<InventorySlot, Item> par in itemPorSlot)
        {
            if (par.Value.nombre == nombreItem)
            {
                slotEncontrado = par.Key;
                break;
            }
        }

        if (slotEncontrado == null) return false;

        inventario.Remove(itemPorSlot[slotEncontrado]);
        itemPorSlot.Remove(slotEncontrado);

        if (!slotEncontrado.EstaVacio)
        {
            Destroy(slotEncontrado.IconoActual.gameObject);
        }
        slotEncontrado.Liberar();
        return true;
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
        itemPorSlot.Clear();
        nombresRecogidos.Clear();
    }

    private void InstanciarIcono(Item item, InventorySlot slot)
    {
        GameObject nuevoIcono = Instantiate(iconoItemPrefab, slot.transform);
        InventoryItemIcon icono = nuevoIcono.GetComponent<InventoryItemIcon>();
        icono.Inicializar(item, slot);
    }
}
