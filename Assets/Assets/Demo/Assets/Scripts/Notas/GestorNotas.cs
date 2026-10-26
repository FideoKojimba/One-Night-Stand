using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra las notas a pantalla completa. Vive en el prefab "Sistemas"
/// (dentro de un Canvas hijo) y es el único que sabe cómo se lee una nota.
///
/// Flujo:
///  - Abrir(): la nota crece desde su origen hasta cubrir la pantalla.
///    El juego queda en pausa (usa GestorDialogo.EnDialogo, igual que los
///    diálogos, así la cámara y los objetos se bloquean solos).
///  - SiguientePagina() / PaginaAnterior(): las muescas inferiores
///    (derecha = avanzar, izquierda = retroceder) cambian de página.
///  - Cerrar(): la flecha superior hace la animación inversa hasta el
///    origen y devuelve el control al juego.
///
/// Para el futuro inventario de notas: Abrir() recibe el origen como
/// un punto de pantalla cualquiera, así un ícono de inventario podría
/// abrir una nota desde su propia posición. La lista notasDescubiertas
/// ya guarda lo que el jugador fue leyendo.
/// </summary>
public class GestorNotas : MonoBehaviour
{
    public static GestorNotas singleton;

    [Header("Elementos de UI")]
    [Tooltip("Panel padre (con CanvasGroup). Contiene la nota, la flecha y la muesca")]
    [SerializeField] private CanvasGroup panelNotas;
    [Tooltip("La imagen que muestra la página actual")]
    [SerializeField] private Image imagenNota;
    [Tooltip("Botón de la flecha semitransparente (arriba)")]
    [SerializeField] private GameObject botonCerrar;
    [Tooltip("Botón de la muesca (abajo a la derecha) para pasar a la página siguiente")]
    [SerializeField] private GameObject botonSiguiente;
    [Tooltip("Botón de la muesca (abajo a la izquierda) para volver a la página anterior")]
    [SerializeField] private GameObject botonAnterior;

    [Header("Sonido")]
    [Tooltip("Suena cada vez que el jugador avanza o retrocede una página")]
    [SerializeField] private AudioClip sonidoCambioPagina;

    [Header("Animación")]
    [SerializeField] private float duracion = 0.4f;
    [Tooltip("Tamaño con el que la nota sale del suelo (0.15 = 15% de su tamaño final)")]
    [SerializeField] private float escalaInicial = 0.15f;

    // Lista de notas que el jugador ya abrió (base para un inventario de notas)
    public List<Nota> notasDescubiertas = new List<Nota>();

    private AudioSource audioSource;
    private RectTransform rectNota;
    private RectTransform rectCanvas;
    private Nota notaActual;
    private int paginaActual;
    private Vector2 origen;          // posición local (en el canvas) desde donde sale
    private Action alTerminarDeCerrar;
    private bool ocupado;            // true mientras anima: ignora clics

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            // No hace falta DontDestroyOnLoad aquí: este objeto es hijo de
            // "Sistemas", cuya raíz ya persiste entre escenas.

            // Fuente de audio propia (se crea sola si el objeto no tiene una)
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            rectNota = imagenNota.rectTransform;
            rectCanvas = panelNotas.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            panelNotas.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Abre una nota. origenPantalla = punto de la pantalla desde donde
    /// sale y adonde vuelve. alTerminarDeCerrar (opcional) se avisa cuando
    /// la nota ya volvió a su sitio.
    /// </summary>
    public void Abrir(Nota nota, Vector2 origenPantalla, Action alTerminarDeCerrar = null)
    {
        if (ocupado || nota == null || nota.paginas == null || nota.paginas.Length == 0) return;

        notaActual = nota;
        paginaActual = 0;
        this.alTerminarDeCerrar = alTerminarDeCerrar;

        if (!notasDescubiertas.Contains(nota)) notasDescubiertas.Add(nota);

        // Convierte el punto de pantalla a posición dentro del canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectCanvas, origenPantalla, null, out origen);

        GestorDialogo.EnDialogo = true; // pausa el juego
        MostrarPagina();

        botonCerrar.SetActive(false);
        botonSiguiente.SetActive(false);
        botonAnterior.SetActive(false);
        panelNotas.gameObject.SetActive(true);

        StartCoroutine(Animar(true));
    }

    /// <summary>Botón de la muesca: pasa a la siguiente página.</summary>
    public void SiguientePagina()
    {
        if (ocupado || notaActual == null) return;

        if (paginaActual < notaActual.paginas.Length - 1)
        {
            paginaActual++;
            MostrarPagina();
            ReproducirSonidoPagina();
        }
    }

    /// <summary>Botón de la muesca izquierda: vuelve a la página anterior.</summary>
    public void PaginaAnterior()
    {
        if (ocupado || notaActual == null) return;

        if (paginaActual > 0)
        {
            paginaActual--;
            MostrarPagina();
            ReproducirSonidoPagina();
        }
    }

    /// <summary>Botón de la flecha: guarda la nota y la devuelve al suelo.</summary>
    public void Cerrar()
    {
        if (ocupado || notaActual == null) return;

        botonCerrar.SetActive(false);
        botonSiguiente.SetActive(false);
        botonAnterior.SetActive(false);
        StartCoroutine(Animar(false));
    }

    /// <summary>
    /// Cierra la nota de golpe, sin animación. Se usa al abandonar la
    /// partida (volver al menú principal) con una nota abierta.
    /// </summary>
    public void Reiniciar()
    {
        StopAllCoroutines();
        ocupado = false;
        notaActual = null;
        alTerminarDeCerrar = null;
        panelNotas.gameObject.SetActive(false);
    }

    // Solo se llama cuando la página REALMENTE cambió (no al estar en
    // la primera o la última), así no suena sin motivo.
    private void ReproducirSonidoPagina()
    {
        if (sonidoCambioPagina != null)
        {
            audioSource.PlayOneShot(sonidoCambioPagina);
        }
    }

    private void MostrarPagina()
    {
        imagenNota.sprite = notaActual.paginas[paginaActual];

        // Cada muesca solo aparece si hay a dónde ir en esa dirección:
        // la derecha si quedan páginas por leer, la izquierda si ya
        // se avanzó y hay páginas anteriores.
        bool hayMas = paginaActual < notaActual.paginas.Length - 1;
        bool hayAnterior = paginaActual > 0;
        botonSiguiente.SetActive(hayMas && !ocupado);
        botonAnterior.SetActive(hayAnterior && !ocupado);
    }

    private IEnumerator Animar(bool abriendo)
    {
        ocupado = true;

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime; // se congela si el juego está en pausa
            float t = Mathf.SmoothStep(0f, 1f, tiempo / duracion);
            if (!abriendo) t = 1f - t; // al cerrar se recorre al revés

            rectNota.anchoredPosition = Vector2.Lerp(origen, Vector2.zero, t);
            rectNota.localScale = Vector3.one * Mathf.Lerp(escalaInicial, 1f, t);
            panelNotas.alpha = t;
            yield return null;
        }

        ocupado = false;

        if (abriendo)
        {
            // Terminó de abrirse: aparecen la flecha y (si hay más) la muesca
            rectNota.anchoredPosition = Vector2.zero;
            rectNota.localScale = Vector3.one;
            panelNotas.alpha = 1f;
            botonCerrar.SetActive(true);
            MostrarPagina();
        }
        else
        {
            // Terminó de cerrarse: se devuelve el control al juego
            panelNotas.gameObject.SetActive(false);
            notaActual = null;
            GestorDialogo.EnDialogo = false;
            alTerminarDeCerrar?.Invoke();
        }
    }
}
