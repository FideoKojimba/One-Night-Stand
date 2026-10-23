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
/// Cómo se avanza: no hay botón. Un clic en CUALQUIER parte de la
/// pantalla avanza el diálogo. Si la línea todavía se está tipeando,
/// el clic la completa de una; si ya estaba completa, pasa a la
/// siguiente. El último clic cierra el diálogo.
/// </summary>
public class GestorDialogo : MonoBehaviour
{
    public static GestorDialogo singleton;

    /// <summary>
    /// True mientras un diálogo está en pantalla. Cualquier otro
    /// script puede consultarla antes de reaccionar a un clic.
    /// </summary>
    public static bool EnDialogo;

    // Las variables static NO se reinician solas si el editor tiene
    // desactivado "Reload Domain". Si detienes el Play en medio de un
    // diálogo, EnDialogo quedaría en true y bloquearía todo el juego
    // en la siguiente prueba. Este método lo reinicia al darle Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarEstadoEstatico()
    {
        EnDialogo = false;
    }

    [Header("Referencias de UI")]
    public GameObject panelDialogo;
    public Image imagenPersonaje;
    [Tooltip("Texto donde se muestra QUIÉN habla (arriba de la caja)")]
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoDialogo;

    [Header("Tiempos")]
    [Tooltip("Segundos de espera antes de mostrar el diálogo, para que termine " +
             "el fundido inicial de la escena")]
    public float retrasoAntesDeMostrar = 0.4f;

    [Tooltip("Segundos de espera entre cada letra que aparece")]
    public float velocidadTipeo = 0.03f;

    private bool avanzar;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);

            // El panel siempre arranca oculto, aunque se haya dejado
            // activo en el prefab por accidente (si no, taparía la
            // pantalla y bloquearía los clics).
            panelDialogo.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Solo cuenta cuando el panel ya está visible: así un clic
        // durante la breve espera inicial no se "guarda" por error.
        if (EnDialogo && panelDialogo.activeSelf && Input.GetMouseButtonDown(0))
        {
            avanzar = true;
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

    private IEnumerator SecuenciaDeDialogo(Sprite personaje, LineaDialogo[] lineas)
    {
        // El juego ya queda pausado desde este instante, aunque el
        // panel todavía no se vea (durante la espera inicial).
        EnDialogo = true;

        yield return new WaitForSeconds(retrasoAntesDeMostrar);

        panelDialogo.SetActive(true);

        // Si este diálogo no tiene personaje, se oculta la imagen por
        // completo (un Image sin sprite se vería como un rectángulo blanco).
        imagenPersonaje.gameObject.SetActive(personaje != null);
        imagenPersonaje.sprite = personaje;

        foreach (LineaDialogo linea in lineas)
        {
            textoNombre.text = linea.nombre;

            yield return StartCoroutine(TipearTexto(linea.texto));

            // Espera un clic NUEVO del jugador antes de pasar a la
            // siguiente línea (se resetea acá para no consumir de
            // arrastre el mismo clic que recién completó el tipeo).
            avanzar = false;
            yield return new WaitUntil(() => avanzar);
        }

        panelDialogo.SetActive(false);
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
                break; // el jugador hizo clic: se completa la línea de una, abajo
            }

            textoDialogo.text += letra;
            yield return new WaitForSeconds(velocidadTipeo);
        }

        textoDialogo.text = texto; // asegura que quede completa, se haya saltado o no
    }
}
