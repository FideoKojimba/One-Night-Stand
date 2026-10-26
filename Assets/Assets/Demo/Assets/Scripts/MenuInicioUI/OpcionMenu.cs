using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Clase BASE para cualquier opción de un menú hecha con texto (TextMeshPro).
/// Se encarga de todo lo que es igual en todas las opciones:
///  - Al pasar el mouse: el texto se pone del color de hover (rojo) y suena
///    un sonido corto de hover.
///  - Al salir el mouse: el texto vuelve a su color original.
///  - Al hacer clic: suena el sonido de confirmación, se espera a que
///    termine de sonar y RECIÉN ENTONCES se ejecuta la acción.
///
/// Lo único que cambia entre opciones es QUÉ hacen al confirmarse, y eso
/// lo define cada clase hija en Ejecutar() (OpcionComenzarJuego,
/// OpcionSalir...). Es el mismo patrón que ObjetoSeleccionable: la clase
/// base hace lo común y las hijas solo agregan lo propio.
///
/// Usa los eventos de UI de Unity (IPointer...), así que necesita un
/// EventSystem en la escena (ya existe dentro de "Sistemas").
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(AudioSource))]
public abstract class OpcionMenu : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Colores")]
    [SerializeField] private Color colorHover = Color.red;

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoHover;
    [SerializeField] private AudioClip sonidoConfirmar;

    private TextMeshProUGUI texto;
    private AudioSource audioSource;
    private Color colorOriginal;
    private bool yaConfirmada; // evita que un doble clic ejecute la acción dos veces

    protected virtual void Awake()
    {
        texto = GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        colorOriginal = texto.color; // se recuerda el color que le pusiste en el editor
    }

    // OnEnable corre cada vez que el objeto se vuelve a activar. En el
    // menú de pausa el panel se apaga y se enciende una y otra vez, y
    // sin esto la opción recordaría que ya fue confirmada y quedaría
    // bloqueada (y con el color de hover) la segunda vez que se abre.
    private void OnEnable()
    {
        yaConfirmada = false;
        texto.color = colorOriginal;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (yaConfirmada) return;

        texto.color = colorHover;
        Reproducir(sonidoHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (yaConfirmada) return;

        texto.color = colorOriginal;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (yaConfirmada) return;

        yaConfirmada = true;
        StartCoroutine(ConfirmarYEjecutar());
    }

    private IEnumerator ConfirmarYEjecutar()
    {
        Reproducir(sonidoConfirmar);

        // Se espera a que el sonido termine para que no se corte
        // si la acción cambia de escena de inmediato.
        if (sonidoConfirmar != null)
        {
            // Realtime: sigue contando aunque el juego esté en pausa
            // (Time.timeScale = 0), como pasa en el menú de pausa.
            yield return new WaitForSecondsRealtime(sonidoConfirmar.length);
        }

        Ejecutar();
    }

    private void Reproducir(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>Lo que hace esta opción al confirmarse. Lo define cada hija.</summary>
    protected abstract void Ejecutar();
}
