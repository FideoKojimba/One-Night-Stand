using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Único punto de control del sistema de diálogos (patrón Singleton,
/// igual que InventoryManager). Persiste entre escenas, así que un
/// diálogo se puede disparar desde cualquier escena sin volver a
/// configurar nada.
///
/// Mientras EnDialogo es true, el resto del juego consulta esta
/// bandera para dejar de reaccionar a clics — así no hace falta
/// pausar el juego con Time.timeScale (que traería complicaciones con
/// las demás coroutines) ni modificar cada script por separado más
/// allá de una sola línea de chequeo.
///
/// Opcionalmente, muestra la escena de fondo difuminada mientras se
/// habla: captura la pantalla, la reduce varias veces a la mitad y la
/// estira de nuevo a pantalla completa (una imagen chica estirada se
/// ve borrosa, sin necesidad de shaders).
/// </summary>
public class GestorDialogo : MonoBehaviour
{
    public static GestorDialogo singleton;

    /// <summary>
    /// True mientras un diálogo está en pantalla. Cualquier otro
    /// script puede consultarla antes de reaccionar a un clic.
    /// </summary>
    public static bool EnDialogo { get; private set; }

    [Header("Referencias de UI")]
    public GameObject panelDialogo;
    public Image imagenPersonaje;
    public TextMeshProUGUI textoDialogo;

    [Header("Fondo difuminado (opcional)")]
    [Tooltip("RawImage a pantalla completa, como PRIMER hijo de PanelDialogo (detrás del " +
             "resto). Si se deja vacío, no hay fondo difuminado.")]
    public RawImage fondoDifuminado;

    [Tooltip("Cuántas veces se reduce a la mitad la captura antes de estirarla. " +
             "Más pases = más difuminado (3 es un buen punto de partida).")]
    [Range(1, 6)]
    public int intensidadDifuminado = 3;

    [Tooltip("Segundos de espera antes de mostrar el diálogo, para que termine el " +
             "fundido inicial de la escena antes de capturar el fondo.")]
    public float retrasoAntesDeMostrar = 0.4f;

    [Header("Velocidad")]
    [Tooltip("Segundos de espera entre cada letra que aparece")]
    public float velocidadTipeo = 0.03f;

    private bool avanzar;
    private RenderTexture texturaDifuminada;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Arranca una secuencia de diálogo. Se puede llamar desde
    /// cualquier objeto (por ejemplo, DialogoTrigger), en cualquier
    /// escena, sin escribir una clase nueva por cada diálogo.
    /// </summary>
    public void MostrarDialogo(Sprite personaje, LineaDialogo[] lineas)
    {
        StartCoroutine(SecuenciaDeDialogo(personaje, lineas));
    }

    /// <summary>
    /// Conectado al botón de "saltar" desde el Inspector. Si la línea
    /// todavía se está tipeando, la completa de una. Si ya estaba
    /// completa, avanza a la siguiente.
    /// </summary>
    public void AlPresionarBotonSaltar()
    {
        avanzar = true;
    }

    private IEnumerator SecuenciaDeDialogo(Sprite personaje, LineaDialogo[] lineas)
    {
        // El juego ya queda pausado desde este instante, aunque el
        // panel todavía no se vea (mientras espera y captura el fondo).
        EnDialogo = true;

        yield return new WaitForSeconds(retrasoAntesDeMostrar);

        if (fondoDifuminado != null)
        {
            yield return StartCoroutine(CapturarFondoDifuminado());
        }

        panelDialogo.SetActive(true);

        // Si este diálogo no tiene personaje, se oculta la imagen por
        // completo (un Image sin sprite se vería como un rectángulo blanco).
        imagenPersonaje.gameObject.SetActive(personaje != null);
        imagenPersonaje.sprite = personaje;

        foreach (LineaDialogo linea in lineas)
        {
            yield return StartCoroutine(TipearTexto(linea.texto));

            // Espera un clic NUEVO del jugador antes de pasar a la
            // siguiente línea (se resetea acá para no consumir de
            // arrastre el mismo clic que recién completó el tipeo).
            avanzar = false;
            yield return new WaitUntil(() => avanzar);
        }

        panelDialogo.SetActive(false);
        LiberarFondoDifuminado();
        EnDialogo = false;
    }

    private IEnumerator TipearTexto(string texto)
    {
        textoDialogo.text = "";
        avanzar = false;

        foreach (char letra in texto)
        {
            if (avanzar)
            {
                break; // el jugador saltó: se completa la línea de una, abajo
            }

            textoDialogo.text += letra;
            yield return new WaitForSeconds(velocidadTipeo);
        }

        textoDialogo.text = texto; // asegura que quede completa, se haya saltado o no
    }

    /// <summary>
    /// Captura lo que se ve en pantalla y lo achica a la mitad varias
    /// veces seguidas (cada paso promedia los píxeles vecinos). Al
    /// mostrar esa imagen chica estirada a pantalla completa, el
    /// resultado se ve difuminado.
    /// </summary>
    private IEnumerator CapturarFondoDifuminado()
    {
        // La captura tiene que hacerse al final del frame, cuando la
        // pantalla ya terminó de dibujarse.
        yield return new WaitForEndOfFrame();

        Texture2D captura = ScreenCapture.CaptureScreenshotAsTexture();

        Texture origen = captura;
        RenderTexture anterior = null;
        int ancho = captura.width;
        int alto = captura.height;

        for (int i = 0; i < intensidadDifuminado; i++)
        {
            ancho = Mathf.Max(1, ancho / 2);
            alto = Mathf.Max(1, alto / 2);

            RenderTexture reducida = new RenderTexture(ancho, alto, 0);
            reducida.filterMode = FilterMode.Bilinear;
            Graphics.Blit(origen, reducida);

            if (anterior != null)
            {
                anterior.Release();
                Destroy(anterior);
            }

            anterior = reducida;
            origen = reducida;
        }

        Destroy(captura);

        LiberarFondoDifuminado();
        texturaDifuminada = anterior;
        fondoDifuminado.texture = texturaDifuminada;
    }

    private void LiberarFondoDifuminado()
    {
        if (fondoDifuminado != null)
        {
            fondoDifuminado.texture = null;
        }

        if (texturaDifuminada != null)
        {
            texturaDifuminada.Release();
            Destroy(texturaDifuminada);
            texturaDifuminada = null;
        }
    }
}
