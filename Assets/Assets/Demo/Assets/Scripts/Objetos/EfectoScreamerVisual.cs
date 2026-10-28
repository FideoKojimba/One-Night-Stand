using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Los EFECTOS VISUALES del screamer (el sonido y el flujo los maneja
/// Screamer). Al llamar a Iniciar():
///  - La imagen SUBE desde abajo hasta cubrir la pantalla.
///  - La imagen crece y decrece (pulsa) cada vez MÁS rápido.
///  - Una luz blanca a pantalla completa destella de forma intermitente.
///
/// Va en el objeto del Canvas del screamer. Todos los valores se ajustan
/// en el Inspector. Usa tiempo real, así que funciona aunque el juego
/// esté pausado.
///
/// AVISO: los destellos rápidos pueden afectar a personas fotosensibles.
/// Las pautas de accesibilidad recomiendan no pasar de 3 destellos por
/// segundo; conviene avisarlo al jugador antes de empezar la demo.
/// </summary>
public class EfectoScreamerVisual : MonoBehaviour
{
    [Header("Elementos de UI")]
    [Tooltip("La imagen del screamer. Debe ocupar toda la pantalla (anclas estiradas) " +
             "y tener el Pivot en el centro (0.5, 0.5).")]
    [SerializeField] private RectTransform imagen;

    [Tooltip("Una Image BLANCA a pantalla completa, ubicada DESPUÉS de la imagen en la " +
             "jerarquía (para quedar encima). Es la luz que destella.")]
    [SerializeField] private Image destelloBlanco;

    [Header("Entrada (de abajo hacia arriba)")]
    [Tooltip("Cuánto tarda la imagen en subir hasta cubrir la pantalla, en segundos")]
    [SerializeField] private float duracionEntrada = 0.12f;

    [Header("Crece y decrece (acelerado)")]
    [Tooltip("Tamaño más chico. 1 = justo la pantalla (no bajes de 1 o se verán bordes).")]
    [SerializeField] private float escalaMinima = 1f;
    [Tooltip("Tamaño más grande. 1.3 = 30% más grande que la pantalla.")]
    [SerializeField] private float escalaMaxima = 1.3f;
    [Tooltip("Pulsos por segundo al empezar")]
    [SerializeField] private float frecuenciaInicial = 6f;
    [Tooltip("Pulsos por segundo al final de la aceleración")]
    [SerializeField] private float frecuenciaFinal = 28f;
    [Tooltip("En cuántos segundos pasa de la frecuencia inicial a la final")]
    [SerializeField] private float segundosParaAcelerar = 1.5f;

    [Header("Destellos blancos")]
    [Tooltip("Cuántas veces por segundo destella la luz")]
    [SerializeField] private float destellosPorSegundo = 8f;
    [Range(0f, 1f)]
    [Tooltip("Qué tan blanca se pone la pantalla en cada destello (1 = blanco total)")]
    [SerializeField] private float intensidadDestello = 0.8f;

    private bool activo;
    private float tiempo;
    private float fase; // ángulo acumulado del pulso (permite acelerar sin saltos)
    private RectTransform padre;

    /// <summary>Empieza los efectos. Llamar justo después de encender el panel.</summary>
    public void Iniciar()
    {
        padre = imagen.parent as RectTransform;
        tiempo = 0f;
        fase = 0f;
        activo = true;

        Aplicar(0f); // deja la imagen ya fuera de pantalla, sin parpadeos de un frame
    }

    private void Update()
    {
        if (!activo) return;

        float dt = Time.unscaledDeltaTime; // tiempo real: no depende de la pausa
        tiempo += dt;
        Aplicar(dt);
    }

    private void Aplicar(float dt)
    {
        // --- Entrada: sube desde abajo con frenado suave al llegar ---
        float k = duracionEntrada > 0f ? Mathf.Clamp01(tiempo / duracionEntrada) : 1f;
        float suave = 1f - (1f - k) * (1f - k) * (1f - k);
        float alto = padre != null ? padre.rect.height : Screen.height;
        imagen.anchoredPosition = new Vector2(0f, -alto * (1f - suave));

        // --- Pulso: la frecuencia sube con el tiempo (por eso se acumula la fase) ---
        float avance = segundosParaAcelerar > 0f ? Mathf.Clamp01(tiempo / segundosParaAcelerar) : 1f;
        float frecuencia = Mathf.Lerp(frecuenciaInicial, frecuenciaFinal, avance);
        fase += frecuencia * Mathf.PI * 2f * dt;
        float onda = Mathf.Sin(fase) * 0.5f + 0.5f; // de 0 a 1
        imagen.localScale = Vector3.one * Mathf.Lerp(escalaMinima, escalaMaxima, onda);

        // --- Destellos: encendido la mitad de cada ciclo, apagado la otra mitad ---
        if (destelloBlanco != null)
        {
            float ciclo = (tiempo * destellosPorSegundo) % 1f;
            Color c = destelloBlanco.color;
            c.r = 1f; c.g = 1f; c.b = 1f;
            c.a = (tiempo > 0f && ciclo < 0.5f) ? intensidadDestello : 0f;
            destelloBlanco.color = c;
        }
    }
}
