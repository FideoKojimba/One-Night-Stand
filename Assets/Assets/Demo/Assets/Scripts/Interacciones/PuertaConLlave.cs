using UnityEngine;

/// <summary>
/// Ejemplo de objeto interactivo: una puerta que se abre si se le da
/// la llave correcta. Es solo UNA implementación posible de
/// IObjetoInteractivo; cualquier otro objeto (cofre, portón, jaula)
/// puede tener su propia clase con su propia lógica sin tocar el
/// sistema de inventario ni el de arrastre.
///
/// Al abrirse, la puerta desaparece por completo y revela otro objeto
/// en su lugar (por ejemplo, una flecha con ObjetoCambioEscena). Ese
/// objeto de reemplazo es genérico a propósito: podría ser una flecha,
/// un cofre destapado, un pasadizo, lo que sea, sin tener que tocar
/// este script para cada puerta nueva que agregues.
///
/// Mientras sigue cerrada, un clic directo (sin la llave) no la abre,
/// pero sí da una respuesta auditiva: un sonido de "puerta trabada",
/// para que el jugador entienda que necesita algo más. Una vez que se
/// desbloquea, este objeto se desactiva (Abrir()) y deja de reaccionar
/// a nada — el objeto que aparece en su lugar ya maneja su propio hover
/// normal con ObjetoCambioEscena.
///
/// El estado "abierta" queda registrado en GestorEstadoJuego, así que
/// si volvés a esta escena después de haber gastado la llave, la
/// puerta aparece directamente abierta en vez de pedirte una llave
/// que ya no tenés.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PuertaConLlave : MonoBehaviour, IObjetoInteractivo
{
    [Tooltip("Debe coincidir exactamente con el nombre del Item que la abre")]
    public string nombreLlaveRequerida = "Llave Dorada";

    [Tooltip("Identificador ÚNICO de esta puerta (distinto para cada puerta del juego). " +
             "Se usa para recordar si ya se abrió, incluso después de recargar la escena.")]
    public string idPuerta = "PuertaSinNombre";

    [Header("Qué pasa al abrirse")]
    [Tooltip("El objeto de la puerta en sí: se desactiva por completo (sprite, collider, todo)")]
    public GameObject visualPuerta;

    [Tooltip("Lo que aparece en su lugar (una flecha, un pasadizo, etc.). Puede dejarse vacío si no aplica.")]
    public GameObject objetoQueAparece;

    [Header("Respuesta al clic mientras está cerrada")]
    [Tooltip("Sonido que se reproduce si hacen clic en la puerta sin haberla desbloqueado todavía")]
    public AudioClip sonidoTrabada;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Si esta puerta ya se abrió antes (en una visita anterior a
        // esta escena), saltamos directo al estado abierto: no tiene
        // sentido pedir una llave que ya se gastó.
        if (GestorEstadoJuego.singleton.YaOcurrio(idPuerta))
        {
            Abrir();
        }
    }

    private void OnMouseDown()
    {
        if (GestorDialogo.EnDialogo) return;

        if (sonidoTrabada != null)
        {
            audioSource.PlayOneShot(sonidoTrabada);
        }
    }

    public bool UsarItem(Item item)
    {
        Debug.Log("[DEBUG] Puerta recibió el ítem: '" + item.nombre + "' (esperaba: '" + nombreLlaveRequerida + "')");

        if (item.nombre != nombreLlaveRequerida)
        {
            return false; // No es la llave correcta: se rechaza
        }

        Abrir();
        GestorEstadoJuego.singleton.MarcarComoOcurrido(idPuerta);
        return true; // Aceptado: la llave se consume
    }

    private void Abrir()
    {
        if (visualPuerta != null) visualPuerta.SetActive(false);
        if (objetoQueAparece != null) objetoQueAparece.SetActive(true);
    }
}
