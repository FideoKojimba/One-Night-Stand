using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Único punto de control para cambiar de escena con efecto de
/// transición. Orquesta tres pasos, en orden:
///   1. Un pequeño acercamiento de la cámara hacia el punto donde se
///      hizo clic (simula "caminar" hacia ahí, ya que el juego es en
///      primera persona y no hay un personaje visible que animar).
///   2. Un fundido rápido a negro.
///   3. Cargar la escena nueva, y fundir de vuelta desde negro.
///
/// Es un Singleton que sobrevive entre escenas (DontDestroyOnLoad),
/// a diferencia de InventoryManager que es Singleton pero vive solo
/// dentro de una escena: este SÍ necesita persistir, porque la
/// transición ocurre justo en el momento en que se cambia de escena.
///
/// Los objetos que antes llamaban a SceneManager.LoadScene() directamente
/// (como FlechaCambioEscena) ahora llaman a este gestor en su lugar.
/// </summary>
public class GestorTransicionEscena : MonoBehaviour
{
    public static GestorTransicionEscena singleton;

    [Header("Panel negro (Image dentro de un Canvas, cubriendo toda la pantalla)")]
    public Image panelNegro;

    [Header("Efecto de caminado (acercamiento de cámara)")]
    [Tooltip("Cuánto se acerca la cámara al punto de destino, en unidades de mundo")]
    public float distanciaCaminado = 1f;
    [Tooltip("Cuánto dura el acercamiento, en segundos")]
    public float duracionCaminado = 0.3f;
    [Tooltip("Cuánto se reduce el Orthographic Size de la cámara (zoom in). " +
             "1 = sin zoom. 0.85 = un 15% más cerca al terminar el caminado.")]
    [Range(0.5f, 1f)]
    public float factorZoom = 0.9f;
    [Tooltip("Cuántos grados se inclina la cámara hacia cada lado, simulando el vaivén de los pasos")]
    public float amplitudBalanceo = 3f;
    [Tooltip("Cuántas veces oscila de un lado a otro durante el caminado (más alto = pasos más rápidos)")]
    public float frecuenciaBalanceo = 2f;

    [Header("Fundido a negro")]
    [Tooltip("Cuánto dura el fundido a negro, y el fundido de regreso, en segundos")]
    public float duracionFundido = 0.25f;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Ya existe uno persistiendo de una escena anterior: este
            // duplicado (por si quedó sin querer en la escena nueva) se
            // destruye, igual que hace InventoryManager con sus duplicados.
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Al entrar a cualquier escena (incluida la primera del juego),
        // nos aseguramos de no arrancar con la pantalla en negro.
        StartCoroutine(Fundido(1f, 0f));
    }

    /// <summary>
    /// Punto de entrada público: llamar esto en vez de
    /// SceneManager.LoadScene() directamente.
    /// </summary>
    /// <param name="nombreEscena">Nombre exacto de la escena a cargar.</param>
    /// <param name="puntoDestino">
    /// Posición del objeto donde se hizo clic (normalmente su
    /// transform.position), usada para saber hacia dónde "caminar".
    /// </param>
    /// <param name="usarEfectoCaminado">
    /// Poné esto en false para los objetos cuyo fondo de destino no
    /// tiene margen de sobra (calza justo con la pantalla): en esos
    /// casos, el acercamiento de cámara revelaría un borde que no
    /// debería verse. El fundido a negro siempre se hace igual.
    /// </param>
    public void IrAEscena(string nombreEscena, Vector3 puntoDestino, bool usarEfectoCaminado = true)
    {
        StartCoroutine(SecuenciaDeTransicion(nombreEscena, puntoDestino, usarEfectoCaminado));
    }

    private IEnumerator SecuenciaDeTransicion(string nombreEscena, Vector3 puntoDestino, bool usarEfectoCaminado)
    {
        if (usarEfectoCaminado)
        {
            yield return StartCoroutine(EfectoCaminado(puntoDestino));
        }

        yield return StartCoroutine(Fundido(0f, 1f));
        SceneManager.LoadScene(nombreEscena);
        yield return StartCoroutine(Fundido(1f, 0f));
    }

    private IEnumerator EfectoCaminado(Vector3 puntoDestino)
    {
        Camera camaraComponente = Camera.main;
        Transform camara = camaraComponente.transform;

        Vector3 posicionInicial = camara.position;
        Vector3 direccion = (puntoDestino - posicionInicial).normalized;
        Vector3 posicionFinal = posicionInicial + direccion * distanciaCaminado;

        float sizeInicial = camaraComponente.orthographicSize;
        float sizeFinal = sizeInicial * factorZoom;

        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < duracionCaminado)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionCaminado;

            // Se acerca al punto de destino
            camara.position = Vector3.Lerp(posicionInicial, posicionFinal, progreso);

            // Zoom in gradual (reduce el Orthographic Size)
            camaraComponente.orthographicSize = Mathf.Lerp(sizeInicial, sizeFinal, progreso);

            // Balanceo: rotación oscilante en Z simulando el vaivén de caminar.
            // Un seno completa un ciclo (izquierda-derecha) por cada vez que
            // "progreso * frecuenciaBalanceo" pasa por un número entero.
            float angulo = Mathf.Sin(progreso * frecuenciaBalanceo * Mathf.PI * 2f) * amplitudBalanceo;
            camara.rotation = Quaternion.Euler(0f, 0f, angulo);

            yield return null;
        }

        // El vaivén siempre vuelve derecho al terminar (el zoom queda
        // aplicado a propósito: se ve más cerca justo antes del corte a negro).
        camara.rotation = Quaternion.identity;
    }

    private IEnumerator Fundido(float alphaInicial, float alphaFinal)
    {
        Color color = panelNegro.color;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionFundido)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionFundido;
            color.a = Mathf.Lerp(alphaInicial, alphaFinal, progreso);
            panelNegro.color = color;
            yield return null;
        }

        color.a = alphaFinal;
        panelNegro.color = color;
    }
}
