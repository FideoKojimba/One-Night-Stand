using System.Collections;
using UnityEngine;

/// <summary>
/// Susto (screamer) de pantalla completa que termina devolviendo al
/// jugador al menú principal. Se dispara con un mini evento del
/// Inspector (por ejemplo, WorldNota > Al Abrir Nota > Screamer > Iniciar()).
///
/// Secuencia:
///   1. (Opcional) Suena un sonido previo, por ejemplo una respiración
///      pesada. Mientras tanto el jugador no puede irse de la escena ni
///      cerrar la nota (la flecha de cerrar se oculta).
///   2. Cuando ese sonido termina, se cierra de golpe cualquier nota o
///      diálogo abierto y aparece la imagen a pantalla completa, con su
///      sonido.
///   3. Cuando el sonido del susto termina (más una pequeña espera
///      opcional), la partida se reinicia y se vuelve al menú principal
///      (lo hace GestorMuerte, igual que al morir).
///
/// El susto es de ESTA escena: su imagen vive en un Canvas propio de la
/// escena, que se mantiene apagado hasta que empieza.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Screamer : MonoBehaviour
{
    [Header("Antes del susto (opcional)")]
    [Tooltip("Sonido que suena primero (ej. respiración pesada). El susto empieza cuando termina. " +
             "Déjalo vacío para que el susto sea inmediato.")]
    [SerializeField] private AudioClip sonidoPrevio;

    [Tooltip("Si está marcado, la música de fondo se apaga cuando empieza el sonido previo, " +
             "para que se oiga solo.")]
    [SerializeField] private bool silenciarMusicaAlEmpezar = true;

    [Header("El susto")]
    [Tooltip("Objeto con el Canvas y la imagen a pantalla completa. Debe estar APAGADO al empezar.")]
    [SerializeField] private GameObject panelScreamer;

    [Tooltip("Sonido del susto")]
    [SerializeField] private AudioClip sonidoScreamer;

    [Tooltip("Segundos que la imagen se queda DESPUÉS de que termine el sonido, antes de volver al menú")]
    [SerializeField] private float esperaFinal = 0.5f;

    private AudioSource audioSource;
    private bool iniciado; // el susto solo ocurre una vez

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Aunque se haya dejado encendido en la escena, arranca oculto
        if (panelScreamer != null) panelScreamer.SetActive(false);
    }

    /// <summary>Empieza la secuencia (mini evento para el Inspector).</summary>
    public void Iniciar()
    {
        if (iniciado) return;
        iniciado = true;

        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        // Durante toda la secuencia no se puede salir de la escena...
        SonidoBloqueaSalida.Bloquear();
        // ...ni cerrar la nota que se está leyendo
        GestorNotas.singleton.BloquearCierre(true);

        // --- 1. Sonido previo (la respiración) ---
        if (silenciarMusicaAlEmpezar) GestorMusica.singleton.Reiniciar();

        if (sonidoPrevio != null)
        {
            audioSource.PlayOneShot(sonidoPrevio);
            // Tiempo real: cuenta bien aunque el jugador pause el juego
            yield return new WaitForSecondsRealtime(sonidoPrevio.length);
        }

        // --- 2. El susto: se cierra todo de golpe y aparece la imagen ---
        GestorNotas.singleton.Reiniciar();      // cierra la nota sin animación
        GestorDialogo.singleton.Reiniciar();    // corta un diálogo en curso
        GestorDialogo.EnDialogo = true;         // bloquea cámara y objetos
        GestorMusica.singleton.Reiniciar();     // música fuera
        InventoryManager.singleton.MostrarInterfaz(false);
        GestorPausa.singleton.MostrarIcono(false);

        if (panelScreamer != null) panelScreamer.SetActive(true);

        audioSource.Stop(); // por si la respiración aún tenía "cola"
        float duracion = 0f;
        if (sonidoScreamer != null)
        {
            audioSource.PlayOneShot(sonidoScreamer);
            duracion = sonidoScreamer.length;
        }

        yield return new WaitForSecondsRealtime(duracion + esperaFinal);

        // --- 3. Se reinicia la partida y se vuelve al menú principal ---
        SonidoBloqueaSalida.Desbloquear();
        GestorMuerte.singleton.ReiniciarPartidaYVolverAlMenu();
    }
}
