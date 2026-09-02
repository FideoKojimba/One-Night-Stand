using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Es la representación VISUAL de un ítem ya guardado en un slot.
/// Se encarga de mostrarse (imagen + texto) y de arrastrarse a sí misma
/// cuando el jugador la agarra con el mouse. No decide EN QUÉ slot
/// termina: eso lo decide el InventorySlot sobre el que se suelta
/// (separación de responsabilidades: el ícono solo sabe moverse,
/// el slot decide si acepta o intercambia).
/// </summary>
public class InventoryItemIcon : ObjetoSeleccionable, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public Image imagen;
    public TextMeshProUGUI texto; // TextMeshPro en vez de Text (UI) clásico

    private Item item;
    private InventorySlot slotAsignado;
    private CanvasGroup canvasGroup;
    private Canvas canvasRaiz;

    public InventorySlot SlotAsignado => slotAsignado;

    protected override void Awake()
    {
        base.Awake(); // guarda la escala original para la animación de hover

        // CanvasGroup se usa para "apagar" el raycast del ícono mientras
        // se arrastra, así el mouse puede detectar el slot que está debajo.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasRaiz = GetComponentInParent<Canvas>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IniciarHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TerminarHover();
    }

    public void Inicializar(Item itemData, InventorySlot slot)
    {
        item = itemData;
        slotAsignado = slot;

        imagen.sprite = item.icono;
        texto.text = item.nombre;

        // Se fuerza la misma posición local exacta (0,0,0) que usan
        // MoverA() y OnEndDrag(), en vez de confiar en la posición que
        // el prefab tenía guardada. Así, la posición inicial y la
        // posición tras un arrastre quedan siempre idénticas.
        transform.SetParent(slot.transform);
        transform.localPosition = Vector3.zero;

        slotAsignado.AsignarIcono(this);
    }

    /// <summary>
    /// Reubica este ícono en otro slot. No pregunta si el slot estaba
    /// libre u ocupado: eso ya lo resolvió InventorySlot.OnDrop antes
    /// de llamar a este método.
    /// </summary>
    public void MoverA(InventorySlot nuevoSlot)
    {
        slotAsignado = nuevoSlot;
        transform.SetParent(nuevoSlot.transform);
        transform.localPosition = Vector3.zero;
        nuevoSlot.AsignarIcono(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        TerminarHover(); // que no quede agrandado mientras se arrastra

        // Deja de bloquear raycasts para que el drop detecte el slot de abajo
        canvasGroup.blocksRaycasts = false;

        // Sube al nivel del Canvas mientras se arrastra, para que se
        // dibuje por encima de los demás slots y no se recorte dentro
        // de su slot original.
        transform.SetParent(canvasRaiz.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Se mueve según el desplazamiento del mouse (delta) en vez de
        // "teletransportar" el ícono a la posición absoluta del cursor.
        // Esto evita el salto visual al inicio del arrastre, ya que
        // conserva exactamente el punto donde el jugador agarró el ícono
        // (si se movía a la posición absoluta, el pivote del ícono se
        // recentraba bajo el cursor de golpe, causando un pequeño salto).
        RectTransform rectPropio = transform as RectTransform;
        rectPropio.anchoredPosition += eventData.delta / canvasRaiz.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // ¿Se soltó sobre un slot de la UI? Si fue así, InventorySlot.OnDrop
        // ya se ejecutó automáticamente antes de este método y reasignó
        // slotAsignado. Lo detectamos revisando qué hay bajo el puntero.
        GameObject objetoBajoPuntero = eventData.pointerCurrentRaycast.gameObject;
        bool soltadoSobreSlot = objetoBajoPuntero != null &&
                                 objetoBajoPuntero.GetComponentInParent<InventorySlot>() != null;

        if (!soltadoSobreSlot && TryUsarSobreObjetoDelMundo(eventData))
        {
            return; // El ítem fue aceptado y consumido; el ícono ya se destruyó
        }

        // En cualquier otro caso (soltado en un slot, o soltado en el
        // vacío, o rechazado por el objeto del mundo): queda en su slot.
        transform.SetParent(slotAsignado.transform);
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Revisa si, en el punto de pantalla donde se soltó el ícono, hay
    /// un objeto del MUNDO del juego (no de la UI) que implemente
    /// IObjetoInteractivo. Si lo hay, le ofrece el ítem.
    /// </summary>
    private bool TryUsarSobreObjetoDelMundo(PointerEventData eventData)
    {
        if (Camera.main == null)
        {
            Debug.Log("[DEBUG] No se encontró una cámara con el tag 'MainCamera'.");
            return false;
        }

        Vector2 puntoMundo = Camera.main.ScreenToWorldPoint(eventData.position);
        Collider2D colisionador = Physics2D.OverlapPoint(puntoMundo);

        if (colisionador == null)
        {
            Debug.Log("[DEBUG] No se encontró ningún Collider2D en el punto donde se soltó el ítem.");
            return false;
        }

        Debug.Log("[DEBUG] Se soltó sobre: " + colisionador.gameObject.name);

        IObjetoInteractivo objetoInteractivo = colisionador.GetComponent<IObjetoInteractivo>();
        if (objetoInteractivo == null)
        {
            Debug.Log("[DEBUG] " + colisionador.gameObject.name + " no tiene ningún componente IObjetoInteractivo.");
            return false;
        }

        bool fueAceptado = objetoInteractivo.UsarItem(item);
        Debug.Log("[DEBUG] UsarItem devolvió: " + fueAceptado);

        if (fueAceptado)
        {
            InventoryManager.singleton.ConsumirItem(item, slotAsignado);
            Destroy(gameObject);
        }

        return fueAceptado;
    }

    /// <summary>
    /// Se mantiene disponible para quitar el ítem del inventario por
    /// completo (por ejemplo, si más adelante quieres soltarlo fuera
    /// de cualquier slot). Actualmente no está conectada a ningún
    /// evento de UI.
    /// </summary>
    public void Remover()
    {
        InventoryManager.singleton.RemoverItem(item, slotAsignado);
        Destroy(gameObject);
    }
}
