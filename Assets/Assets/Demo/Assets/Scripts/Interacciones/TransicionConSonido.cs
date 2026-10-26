using UnityEngine;

/// <summary>
/// Cambia de escena con un sonido: al llamar a Iniciar(), suena el sonido
/// y el fundido a negro dura EXACTAMENTE lo mismo que el sonido, así que
/// la escena nueva aparece justo cuando el sonido termina (y el sonido
/// nunca se corta).
///
/// Es una pieza genérica para los mini eventos del Inspector: se conecta
/// a cualquier cosa que quiera "hacer ruido y cambiar de escena" (usar
/// una pala, recoger un objeto, abrir una trampilla...).
///
/// Opcionalmente deja registrado un evento del juego al empezar, para que
/// otras escenas puedan reaccionar (por ejemplo, que la tierra ya no esté
/// al volver, usando MostrarSiOcurrioEvento con ese mismo id).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TransicionConSonido : MonoBehaviour
{
    [Tooltip("Sonido que suena durante la transición. El fundido a negro dura lo mismo que él.")]
    [SerializeField] private AudioClip sonido;

    [Tooltip("Nombre EXACTO de la escena a la que se va al terminar el sonido (debe estar en Build Settings)")]
    [SerializeField] private string escenaDestino;

    [Tooltip("Si quieres además el efecto de caminado antes del fundido")]
    [SerializeField] private bool usarEfectoCaminado = false;

    [Tooltip("(Opcional) Id de un evento que se marca como ocurrido al empezar la transición")]
    [SerializeField] private string idEventoAlIniciar = "";

    private AudioSource audioSource;
    private bool iniciada; // evita disparar la transición dos veces

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>Empieza el sonido y la transición hacia la escena destino.</summary>
    public void Iniciar()
    {
        if (iniciada) return;
        iniciada = true;

        if (!string.IsNullOrEmpty(idEventoAlIniciar))
        {
            GestorEstadoJuego.singleton.MarcarComoOcurrido(idEventoAlIniciar);
        }

        float duracionFundido = -1f; // -1 = fundido normal, si no hay sonido
        if (sonido != null)
        {
            audioSource.PlayOneShot(sonido);
            duracionFundido = sonido.length;
        }

        GestorTransicionEscena.singleton.IrAEscena(escenaDestino, transform.position, usarEfectoCaminado, duracionFundido);
    }
}
