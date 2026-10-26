using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Muestra u oculta un objeto según si un evento del juego YA ocurrió,
/// aunque ese evento haya pasado en OTRA escena.
///
/// Por qué existe: los mini eventos del Inspector (UnityEvent) solo
/// pueden conectarse con objetos de la MISMA escena. Para cosas que
/// aparecen en una escena distinta, la comunicación pasa por
/// GestorEstadoJuego, que recuerda los eventos entre escenas:
///   - Escena A: ObjetoReceptorDeItem marca el evento (campo Id Evento).
///   - Escena B: este script lee ese mismo id al cargar y decide si
///     el objeto aparece.
///
/// Este script va en un objeto SIEMPRE ACTIVO (por ejemplo, un vacío),
/// no en el objeto a mostrar: un objeto desactivado no ejecuta su Awake,
/// así que no podría activarse a sí mismo.
///
/// Además, al entrar a la escena con el evento ya ocurrido, dispara la
/// lista "Al Entrar Con Evento Ocurrido": ahí se conectan otras piezas
/// de ESTA escena (por ejemplo, un diálogo con DialogoTrigger >
/// MostrarDialogo). Si esa pieza está marcada "solo una vez", no se
/// repite aunque se vuelva a entrar.
///
/// Es genérico: sirve para cualquier cosa que deba aparecer (o
/// desaparecer) o suceder después de un evento, no solo para trampillas.
/// </summary>
public class MostrarSiOcurrioEvento : MonoBehaviour
{
    [Tooltip("El objeto que aparece (por ejemplo, la trampilla). Puede estar desactivado.")]
    [SerializeField] private GameObject objetoAControlar;

    [Tooltip("Debe ser IGUAL al Id Evento del objeto que dispara el evento (ej. 'SangrePuesta').")]
    [SerializeField] private string idEvento;

    [Tooltip("Activado: el objeto APARECE cuando el evento ocurrió. " +
             "Desactivado: el objeto DESAPARECE cuando el evento ocurrió.")]
    [SerializeField] private bool apareceAlOcurrir = true;

    [Tooltip("Qué ocurre al ENTRAR a esta escena si el evento YA ocurrió " +
             "(ej.: DialogoTrigger > MostrarDialogo, AudioSource > Play).")]
    [SerializeField] private UnityEvent alEntrarConEventoOcurrido;

    private void Awake()
    {
        bool ocurrio = GestorEstadoJuego.singleton.YaOcurrio(idEvento);

        // Si "apareceAlOcurrir" es true: visible = ocurrio.
        // Si es false: visible = lo contrario.
        objetoAControlar.SetActive(ocurrio == apareceAlOcurrir);
    }

    // Start (y no Awake) para que los demás objetos de la escena ya
    // estén listos cuando se dispare el evento (por ejemplo, el diálogo).
    private void Start()
    {
        if (GestorEstadoJuego.singleton.YaOcurrio(idEvento))
        {
            alEntrarConEventoOcurrido.Invoke();
        }
    }
}
