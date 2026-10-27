using UnityEngine;

/// <summary>
/// Cambia de escena con un sonido. Tiene DOS modos (casilla "Sonar En Negro"):
///
///  - Desmarcada (por defecto): suena el sonido y el fundido a negro dura
///    EXACTAMENTE lo mismo que el sonido, así la escena nueva aparece justo
///    cuando el sonido termina (y el sonido nunca se corta).
///
///  - Marcada: el fundido es CORTO (el normal del juego), y el sonido suena
///    con la pantalla ya en NEGRO. La escena nueva se carga cuando el sonido
///    termina. Sirve para sonidos "en la oscuridad" (un corte, un quejido...).
///
/// Es una pieza genérica para los mini eventos del Inspector: se conecta
/// a cualquier cosa que quiera "hacer ruido y cambiar de escena" (usar
/// una pala, recoger un objeto, abrir una trampilla...). Si la escena
/// destino es la MISMA donde está, la recarga (útil para cambiar su estado
/// usando MostrarSiOcurrioEvento).
///
/// Opcionalmente deja registrado un evento del juego al empezar, para que
/// otras escenas puedan reaccionar (por ejemplo, que la tierra ya no esté
/// al volver, usando MostrarSiOcurrioEvento con ese mismo id).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TransicionConSonido : MonoBehaviour
{
    [Tooltip("Sonido que suena durante la transición (puede quedar vacío)")]
    [SerializeField] private AudioClip sonido;

    [Tooltip("Nombre EXACTO de la escena a la que se va (debe estar en Build Settings)")]
    [SerializeField] private string escenaDestino;

    [Tooltip("Desmarcado: el fundido dura lo mismo que el sonido. " +
             "Marcado: fundido corto y el sonido suena con la pantalla en negro.")]
    [SerializeField] private bool sonarEnNegro = false;

    [Tooltip("Si quieres además el efecto de caminado antes del fundido")]
    [SerializeField] private bool usarEfectoCaminado = false;

    [Tooltip("(Opcional) Id de un evento que se marca como ocurrido al empezar la transición")]
    [SerializeField] private string idEventoAlIniciar = "";

    private AudioSource audioSource;
    private bool iniciada; // evita disparar la transición dos veces

    private void Awake()
    {
        ObtenerAudio();
    }

    // El AudioSource se pide cuando hace falta (y no solo en Awake), así
    // funciona aunque este objeto haya estado desactivado hasta ahora.
    private AudioSource ObtenerAudio()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        return audioSource;
    }

    /// <summary>Empieza la transición hacia la escena destino (con su sonido).</summary>
    public void Iniciar()
    {
        if (iniciada) return;

        // Avisa con un error claro (en vez de quedarse trabado en negro) si la
        // escena no existe o no está en Build Settings.
        if (string.IsNullOrEmpty(escenaDestino) || !Application.CanStreamedLevelBeLoaded(escenaDestino))
        {
            Debug.LogError("[TransicionConSonido] La escena '" + escenaDestino + "' no existe o no está " +
                           "en Build Settings (File > Build Profiles > Scene List). Objeto: " + name, this);
            return;
        }

        iniciada = true;

        if (!string.IsNullOrEmpty(idEventoAlIniciar))
        {
            GestorEstadoJuego.singleton.MarcarComoOcurrido(idEventoAlIniciar);
        }

        GestorTransicionEscena gestor = GestorTransicionEscena.singleton;

        if (sonido != null && sonarEnNegro)
        {
            // Fundido normal; con la pantalla en negro suena el sonido y se espera a que termine
            gestor.IrAEscena(escenaDestino, transform.position, usarEfectoCaminado,
                             -1f, sonido.length, ReproducirSonido);
            return;
        }

        float duracionFundido = -1f; // -1 = fundido normal, si no hay sonido
        if (sonido != null)
        {
            ObtenerAudio().PlayOneShot(sonido);
            duracionFundido = sonido.length;
        }

        gestor.IrAEscena(escenaDestino, transform.position, usarEfectoCaminado, duracionFundido);
    }

    private void ReproducirSonido()
    {
        // Por si este objeto ya no existe cuando la pantalla llega a negro
        if (this == null) return;

        ObtenerAudio().PlayOneShot(sonido);
    }
}
