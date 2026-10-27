using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la secuencia de MUERTE de Blair. Vive en el prefab "Sistemas"
/// (dentro del Canvas) para poder activarse desde cualquier escena.
///
/// Secuencia (todos los tiempos se pueden ajustar en el Inspector):
///   1. El juego se congela (no se puede interactuar, ni pausar) y la
///      música se detiene.
///   2. Aparece la IMAGEN 1 de la muerte. Durante el primer segundo no
///      suena nada; al segundo 1 empieza el audio. La imagen 1 dura
///      1.5 segundos en total.
///   3. Aparece la IMAGEN 2 y se queda 1 segundo.
///   4. Un fondo negro aparece y cubre toda la pantalla, hasta que el
///      audio termina de sonar.
///   5. Se reinicia la partida y se vuelve al menú principal.
///
/// Esta clase solo sabe CÓMO se muere. Quién decide CUÁNDO muere Blair lo
/// decide cada escena, con el script DisparadorDeMuerte conectado a un
/// mini evento del Inspector.
/// </summary>
public class GestorMuerte : MonoBehaviour
{
    public static GestorMuerte singleton;

    [Header("Elementos de UI (dentro del Canvas de Sistemas)")]
    [Tooltip("Panel padre de la secuencia. Se mantiene apagado hasta que Blair muere")]
    [SerializeField] private GameObject panelMuerte;
    [Tooltip("La imagen a pantalla completa donde se muestran las dos imágenes de muerte")]
    [SerializeField] private Image imagenMuerte;
    [Tooltip("Image negro que cubre toda la pantalla al final. Debe estar DESPUÉS de ImagenMuerte en la jerarquía, para quedar encima y taparla.")]
    [SerializeField] private Image fondoNegro;

    [Header("Contenido")]
    [SerializeField] private Sprite imagenMuerte1;
    [SerializeField] private Sprite imagenMuerte2;
    [SerializeField] private AudioClip sonidoMuerte;

    [Header("Tiempos (en segundos)")]
    [Tooltip("Cuánto tarda en empezar el audio desde que aparece la imagen 1 (silencio inicial)")]
    [SerializeField] private float silencioInicial = 1f;
    [Tooltip("Cuánto dura en total la imagen 1")]
    [SerializeField] private float duracionImagen1 = 1.5f;
    [Tooltip("Cuánto dura la imagen 2")]
    [SerializeField] private float duracionImagen2 = 1f;
    [Tooltip("Cuánto tarda el fondo negro en cubrir la pantalla")]
    [SerializeField] private float duracionFundidoNegro = 0.5f;

    [Header("Destino")]
    [Tooltip("Nombre EXACTO de la escena del menú principal (debe estar en Build Settings)")]
    [SerializeField] private string escenaMenu;

    /// <summary>True mientras la secuencia de muerte está en curso.</summary>
    public static bool EnMuerte { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            EnMuerte = false;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            panelMuerte.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Inicia la secuencia de muerte. Se ignora si ya está en curso.</summary>
    public void Morir()
    {
        if (EnMuerte) return;

        StartCoroutine(SecuenciaDeMuerte());
    }

    /// <summary>
    /// Reinicia toda la partida (cuenta regresiva, inventario, eventos) y vuelve
    /// al menú principal. La usa la secuencia de muerte y también el Screamer,
    /// para que "cómo se vuelve al menú" esté escrito en un solo lugar.
    /// </summary>
    public void ReiniciarPartidaYVolverAlMenu()
    {
        if (ContadorRegresivo.singleton != null) ContadorRegresivo.singleton.Reiniciar();
        InventoryManager.singleton.Reiniciar();
        GestorEstadoJuego.singleton.Reiniciar();
        GestorDialogo.EnDialogo = false;

        GestorTransicionEscena.singleton.IrAEscena(escenaMenu, Vector3.zero, false);
    }

    private IEnumerator SecuenciaDeMuerte()
    {
        EnMuerte = true;

        // --- 1. Se congela todo: nada interactúa, ni se puede pausar ---
        GestorDialogo.singleton.Reiniciar();   // corta un diálogo en curso
        GestorNotas.singleton.Reiniciar();     // cierra una nota abierta
        GestorDialogo.EnDialogo = true;        // bloquea cámara y objetos
        GestorMusica.singleton.Reiniciar();    // la música se detiene
        InventoryManager.singleton.MostrarInterfaz(false);
        GestorPausa.singleton.MostrarIcono(false);

        // --- 2. Imagen 1: silencio, y al segundo 1 empieza el audio ---
        Color negro = fondoNegro.color;
        negro.a = 0f;
        fondoNegro.color = negro;

        imagenMuerte.sprite = imagenMuerte1;
        panelMuerte.SetActive(true);

        yield return new WaitForSecondsRealtime(silencioInicial);

        audioSource.PlayOneShot(sonidoMuerte);

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, duracionImagen1 - silencioInicial));

        // --- 3. Imagen 2 ---
        imagenMuerte.sprite = imagenMuerte2;

        yield return new WaitForSecondsRealtime(duracionImagen2);

        // --- 4. El fondo negro cubre la pantalla hasta que acabe el audio ---
        float tiempo = 0f;
        while (tiempo < duracionFundidoNegro)
        {
            tiempo += Time.unscaledDeltaTime;
            negro.a = Mathf.Clamp01(tiempo / duracionFundidoNegro);
            fondoNegro.color = negro;
            yield return null;
        }
        negro.a = 1f;
        fondoNegro.color = negro;

        yield return new WaitWhile(() => audioSource.isPlaying);

        // --- 5. Se reinicia la partida y se vuelve al menú principal ---
        ReiniciarPartidaYVolverAlMenu();

        // Se espera a que el fundido del gestor de transición tape todo
        // (y la escena cambie) antes de apagar este panel.
        yield return new WaitForSecondsRealtime(GestorTransicionEscena.singleton.duracionFundido + 0.1f);

        panelMuerte.SetActive(false);
        EnMuerte = false;
    }
}
