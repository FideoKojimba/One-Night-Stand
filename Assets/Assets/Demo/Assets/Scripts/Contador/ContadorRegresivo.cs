using TMPro;
using UnityEngine;

/// <summary>
/// Cuenta regresiva del juego. Vive en el prefab "Sistemas" para que siga
/// corriendo al cambiar de escena. Cuando llega a cero, Blair MUERE
/// (GestorMuerte), sin importar lo que esté pasando en ese momento.
///
/// Esta clase solo sabe contar y avisar. NO decide cuándo empieza ni
/// cuándo termina: eso lo deciden las escenas, con el script
/// DisparadorDeContador conectado a mini eventos del Inspector
/// (por ejemplo, empezar al terminar el diálogo inicial y detenerse
/// cuando Blair pone la sangre).
///
/// - Usa Time.deltaTime, así que se congela mientras el juego está en
///   pausa, pero sigue corriendo durante diálogos y notas.
/// - El texto en pantalla es OPCIONAL: si no lo asignas, la cuenta
///   corre "en secreto".
/// </summary>
public class ContadorRegresivo : MonoBehaviour
{
    public static ContadorRegresivo singleton;

    [Header("Tiempo")]
    [Tooltip("Duración total de la cuenta regresiva, en segundos (180 = 3 minutos)")]
    [SerializeField] private float duracionSegundos = 180f;

    [Header("Texto en pantalla (opcional)")]
    [Tooltip("Texto donde se muestra el tiempo restante (mm:ss). Déjalo vacío para no mostrar nada.")]
    [SerializeField] private TextMeshProUGUI textoContador;

    [Header("Música del último tramo")]
    [Tooltip("Canción que interrumpe a todas las demás cuando queda poco tiempo")]
    [SerializeField] private AudioClip musicaUltimoMinuto;
    [Tooltip("Cuántos segundos antes del final empieza esa canción (60 = último minuto)")]
    [SerializeField] private float segundosParaMusica = 60f;

    private float tiempoRestante;
    private bool musicaActivada; // para que la canción se dispare una sola vez
    private bool activo;
    private bool terminado; // true cuando ya se detuvo (sangre puesta): no se puede reiniciar

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            tiempoRestante = duracionSegundos;
            OcultarTexto();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Empieza la cuenta. Se ignora si ya está corriendo o ya se detuvo.</summary>
    public void Iniciar()
    {
        if (activo || terminado) return;

        tiempoRestante = duracionSegundos;
        musicaActivada = false;
        activo = true;

        if (textoContador != null)
        {
            textoContador.gameObject.SetActive(true);
            ActualizarTexto();
        }
    }

    /// <summary>Detiene la cuenta para siempre (hasta reiniciar la partida).</summary>
    public void Detener()
    {
        activo = false;
        terminado = true;
        OcultarTexto();

        // La cuenta terminó: las escenas pueden volver a manejar la música
        if (GestorMusica.singleton != null)
        {
            GestorMusica.singleton.LiberarMusicaForzada();
        }
    }

    /// <summary>Deja todo como al principio. Se usa al reiniciar la partida.</summary>
    public void Reiniciar()
    {
        activo = false;
        terminado = false;
        musicaActivada = false;
        tiempoRestante = duracionSegundos;
        OcultarTexto();
    }

    private void Update()
    {
        if (!activo) return;

        // Si Blair ya está muriendo por otra causa, la cuenta no sigue
        if (GestorMuerte.EnMuerte)
        {
            Detener();
            return;
        }

        tiempoRestante -= Time.deltaTime;
        ActualizarTexto();

        // Cuando queda poco tiempo, suena la canción especial (una sola vez)
        if (!musicaActivada && tiempoRestante <= segundosParaMusica)
        {
            musicaActivada = true;
            GestorMusica.singleton.ForzarMusica(musicaUltimoMinuto);
        }

        if (tiempoRestante <= 0f)
        {
            Detener();
            GestorMuerte.singleton.Morir();
        }
    }

    private void ActualizarTexto()
    {
        if (textoContador == null) return;

        int total = Mathf.Max(0, Mathf.CeilToInt(tiempoRestante));
        textoContador.text = string.Format("{0:00}:{1:00}", total / 60, total % 60);
    }

    private void OcultarTexto()
    {
        if (textoContador != null)
        {
            textoContador.gameObject.SetActive(false);
        }
    }
}
