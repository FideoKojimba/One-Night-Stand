using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objeto genérico que acepta un ítem específico arrastrado desde el
/// inventario (por ejemplo, una llave, una bolsa de sangre, una pieza
/// que falta en una máquina). Al aceptarlo: cambia su propio sprite,
/// espera un momento, dispara los EVENTOS que le configures en el
/// Inspector (un sonido, mostrar un objeto, un diálogo...) y, si le
/// pusiste una escena destino, hace la transición hacia ella.
///
/// Cada "mini evento" (sonido, aparecer un objeto, diálogo) NO está
/// programado acá: son piezas que ya existen (AudioSource,
/// GameObject.SetActive, DialogoTrigger.MostrarDialogo) y se conectan
/// desde el Inspector con la lista "Al Aceptar". Así cada pieza se
/// reutiliza en cualquier otro objeto sin escribir código nuevo.
///
/// Reutiliza IObjetoInteractivo — la misma interfaz que ya usa
/// PuertaConLlave — y GestorTransicionEscena para el fundido y el
/// cambio de escena. Pensado para reusarse en cualquier objeto del
/// juego que siga este mismo patrón: "recibe esto, se transforma,
/// avanza a otro lado", sin tener que escribir una clase nueva cada
/// vez.
/// </summary>
public class ObjetoReceptorDeItem : MonoBehaviour, IObjetoInteractivo
{
    [Tooltip("Debe coincidir exactamente con el nombre del Item que este objeto acepta")]
    public string nombreItemRequerido = "Item Requerido";

    [Header("Qué pasa al aceptarlo")]
    [Tooltip("Sprite al que cambia este mismo objeto al aceptar el ítem")]
    public Sprite spriteAlAceptar;

    [Tooltip("Cuánto espera (en segundos) después de cambiar el sprite, antes de fundir a negro")]
    public float tiempoDeEspera = 1.5f;

    [Header("Al hacer clic sobre este objeto (opcional)")]
    [Tooltip("Sonido que suena en CADA clic sobre el objeto (ej. un sonido de tierra), " +
             "tenga o no tenga el ítem. Suena junto con el diálogo.")]
    public AudioClip sonidoAlTocar;

    [Tooltip("Diálogo que sale si el jugador hace clic y NO tiene todavía el ítem requerido " +
             "(ej. 'no puedo excavar con las manos'). Debe tener 'Al Iniciar La Escena' y " +
             "'Solo Una Vez' desmarcados, para que se repita en cada clic.")]
    public DialogoTrigger dialogoSinItem;

    [Tooltip("Diálogo que sale si el jugador hace clic y YA tiene el ítem requerido en el " +
             "inventario, pero todavía no lo usó (ej. 'puedo usar la pala para abrir este boquete').")]
    public DialogoTrigger dialogoConItem;

    [Header("Mini eventos (se conectan en el Inspector)")]
    [Tooltip("Todo lo que ocurre al aceptar el ítem, en orden: reproducir un sonido " +
             "(AudioSource > Play), mostrar un objeto (GameObject > SetActive), " +
             "lanzar un diálogo (DialogoTrigger > MostrarDialogo)...")]
    public UnityEvent alAceptar;

    [Tooltip("(Opcional) Identificador ÚNICO de este evento. Si lo llenás, se recuerda que " +
             "ya ocurrió, así que al volver a esta escena el objeto aparece ya resuelto.")]
    public string idEvento = "";

    [Tooltip("Qué se muestra al volver a la escena si el evento YA ocurrió " +
             "(por ejemplo, dejar visible la trampilla). Sin sonido ni diálogo.")]
    public UnityEvent alVolverConEventoCompletado;

    [Header("Cambio de escena (opcional)")]
    [Tooltip("Escena a la que se llega después del fundido. Dejalo VACÍO si no querés cambiar de escena.")]
    public string escenaDestino;

    [Tooltip("Si querés el efecto de caminado al ir hacia la escena destino")]
    public bool usarEfectoCaminado = false;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool yaAceptado; // una vez aceptado el ítem, los clics ya no hacen nada

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Fuente de audio propia, solo si hay sonido de clic
        if (sonidoAlTocar != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Si este evento ya ocurrió en una visita anterior, el objeto
        // aparece directamente en su estado final, sin repetir nada.
        if (!string.IsNullOrEmpty(idEvento) && GestorEstadoJuego.singleton.YaOcurrio(idEvento))
        {
            yaAceptado = true;
            if (spriteAlAceptar != null) spriteRenderer.sprite = spriteAlAceptar;
            alVolverConEventoCompletado.Invoke();
        }
    }

    // Clic directo sobre el objeto: Blair comenta según tenga o no el ítem
    private void OnMouseDown()
    {
        if (yaAceptado || GestorDialogo.EnDialogo) return;

        if (sonidoAlTocar != null)
        {
            audioSource.PlayOneShot(sonidoAlTocar);
        }

        bool tieneElItem = InventoryManager.singleton.TieneItem(nombreItemRequerido);
        DialogoTrigger dialogo = tieneElItem ? dialogoConItem : dialogoSinItem;

        if (dialogo != null)
        {
            dialogo.MostrarDialogo();
        }
    }

    public bool UsarItem(Item item)
    {
        if (item.nombre != nombreItemRequerido)
        {
            return false; // No es el ítem correcto: se rechaza
        }

        if (yaAceptado) return false; // ya se aceptó antes: no se acepta dos veces

        yaAceptado = true;
        StartCoroutine(SecuenciaDeAceptacion());
        return true; // Aceptado: el ítem se consume
    }

    private IEnumerator SecuenciaDeAceptacion()
    {
        if (spriteAlAceptar != null)
        {
            spriteRenderer.sprite = spriteAlAceptar;
        }

        yield return new WaitForSeconds(tiempoDeEspera);

        if (!string.IsNullOrEmpty(idEvento))
        {
            GestorEstadoJuego.singleton.MarcarComoOcurrido(idEvento);
        }

        alAceptar.Invoke();

        // Solo cambia de escena si se configuró una
        if (!string.IsNullOrEmpty(escenaDestino))
        {
            GestorTransicionEscena.singleton.IrAEscena(escenaDestino, transform.position, usarEfectoCaminado);
        }
    }
}
