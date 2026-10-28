using System.Collections;
using UnityEngine;

/// <summary>
/// Susto (screamer) de pantalla completa, en DOS momentos que se conectan
/// desde el Inspector a la nota:
///
///   1. EmpezarMurmullos()  (WorldNota > Al Abrir Nota)
///      Al recoger la nota empiezan unos murmullos que se repiten sin
///      parar. El jugador puede seguir leyendo todo el tiempo que quiera.
///
///   2. Asustar()           (WorldNota > Al Pulsar Cerrar)
///      En el instante en que el jugador decide cerrar la nota, los
///      murmullos se cortan, la nota se cierra de golpe y aparece el
///      screamer: la imagen sube desde abajo, pulsa muy rápido y la
///      pantalla destella en blanco (EfectoScreamerVisual), con su sonido.
///      Cuando el sonido termina (más una espera opcional), la partida se
///      reinicia y se vuelve al menú principal (lo hace GestorMuerte).
///
/// Sobre el volumen: el sonido del susto se reproduce al volumen que
/// configures, con un TOPE de seguridad para que nunca llegue a ser dañino.
/// Para que "suene duro" sin reventar oídos, lo mejor es controlar el
/// volumen aquí (y normalizar el audio a -3 dB) en vez de subir el archivo.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Screamer : MonoBehaviour
{
    // Tope de seguridad: ningún valor del Inspector supera esto
    private const float volumenMaximoSeguro = 0.9f;

    [Header("Murmullos (mientras lee la nota)")]
    [Tooltip("Sonido que se repite en bucle desde que se abre la nota hasta que se cierra")]
    [SerializeField] private AudioClip murmullos;

    [Range(0f, 1f)]
    [SerializeField] private float volumenMurmullos = 0.6f;

    [Tooltip("Si está marcado, la música de fondo se apaga cuando empiezan los murmullos")]
    [SerializeField] private bool silenciarMusicaAlEmpezar = true;

    [Header("El susto")]
    [Tooltip("Objeto con el Canvas del screamer (con EfectoScreamerVisual). Debe estar APAGADO al empezar.")]
    [SerializeField] private GameObject panelScreamer;

    [Tooltip("Los efectos visuales del screamer (subir, pulsar, destellos)")]
    [SerializeField] private EfectoScreamerVisual efectoVisual;

    [Tooltip("Sonido del susto")]
    [SerializeField] private AudioClip sonidoScreamer;

    [Range(0f, 1f)]
    [Tooltip("Qué tan fuerte suena el susto. Se limita a un máximo de seguridad (0.9).")]
    [SerializeField] private float volumenScreamer = 0.8f;

    [Tooltip("Segundos que el susto sigue DESPUÉS de que termine el sonido, antes de volver al menú")]
    [SerializeField] private float esperaFinal = 0.5f;

    private AudioSource fuenteScreamer;
    private AudioSource fuenteMurmullos;
    private bool murmullosIniciados;
    private bool asustado; // el susto solo ocurre una vez

    private void Awake()
    {
        fuenteScreamer = GetComponent<AudioSource>();
        fuenteScreamer.playOnAwake = false;

        // Una segunda fuente aparte: los murmullos van en bucle y no deben mezclarse con el susto
        fuenteMurmullos = gameObject.AddComponent<AudioSource>();
        fuenteMurmullos.playOnAwake = false;
        fuenteMurmullos.loop = true;

        // Aunque se haya dejado encendido en la escena, arranca oculto
        if (panelScreamer != null) panelScreamer.SetActive(false);
    }

    /// <summary>Empiezan los murmullos en bucle (mini evento: Al Abrir Nota).</summary>
    public void EmpezarMurmullos()
    {
        if (murmullosIniciados || asustado) return;
        murmullosIniciados = true;

        if (silenciarMusicaAlEmpezar) GestorMusica.singleton.Reiniciar();

        if (murmullos != null)
        {
            fuenteMurmullos.clip = murmullos;
            fuenteMurmullos.volume = volumenMurmullos;
            fuenteMurmullos.Play();
        }
    }

    /// <summary>¡PUM! (mini evento: Al Pulsar Cerrar).</summary>
    public void Asustar()
    {
        if (asustado) return;
        asustado = true;

        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        // No se puede salir de la escena durante el susto
        SonidoBloqueaSalida.Bloquear();

        // Se corta todo de golpe
        fuenteMurmullos.Stop();
        GestorNotas.singleton.Reiniciar();      // cierra la nota sin animación
        GestorDialogo.singleton.Reiniciar();    // corta un diálogo en curso
        GestorDialogo.EnDialogo = true;         // bloquea cámara y objetos
        GestorMusica.singleton.Reiniciar();     // música fuera
        InventoryManager.singleton.MostrarInterfaz(false);
        GestorPausa.singleton.MostrarIcono(false);

        // Aparece el susto (imagen, destellos) y suena
        if (panelScreamer != null) panelScreamer.SetActive(true);
        if (efectoVisual != null) efectoVisual.Iniciar();

        float duracion = 0f;
        if (sonidoScreamer != null)
        {
            float volumen = Mathf.Min(volumenScreamer, volumenMaximoSeguro);
            fuenteScreamer.PlayOneShot(sonidoScreamer, volumen);
            duracion = sonidoScreamer.length;
        }

        // Tiempo real: cuenta bien aunque el jugador pause el juego
        yield return new WaitForSecondsRealtime(duracion + esperaFinal);

        // Se reinicia la partida y se vuelve al menú principal
        SonidoBloqueaSalida.Desbloquear();
        GestorMuerte.singleton.ReiniciarPartidaYVolverAlMenu();
    }
}
