using UnityEngine;

/// <summary>
/// Controla la pausa del juego. Vive en el prefab "Sistemas" (dentro del
/// Canvas), junto al ícono de pausa y al panel del menú de pausa.
///
/// Se puede pausar en cualquier momento (también durante diálogos y notas).
/// Al pausar:
///  - Congela el tiempo (Time.timeScale = 0).
///  - Bloquea la cámara y los objetos del mundo usando
///    GestorDialogo.EnDialogo, igual que los diálogos y las notas.
///  - Oculta el ícono de pausa y la barra del inventario, y muestra
///    el panel con las opciones.
/// Reanudar() deshace todo eso.
///
/// Las opciones del menú (Continuar, Volver al menú, Salir) son clases
/// aparte que heredan de OpcionMenu; esta clase solo sabe pausar y reanudar.
/// </summary>
public class GestorPausa : MonoBehaviour
{
    public static GestorPausa singleton;

    [Header("Elementos de UI")]
    [Tooltip("El ícono de arriba a la izquierda que abre la pausa")]
    [SerializeField] private GameObject botonPausa;
    [Tooltip("El panel con el fondo oscuro y las tres opciones")]
    [SerializeField] private GameObject panelPausa;

    /// <summary>True mientras el juego está pausado. Otros scripts lo consultan.</summary>
    public static bool EnPausa { get; private set; }

    // Recuerda si ya había un diálogo o una nota abierta al pausar,
    // para dejar todo igual al reanudar.
    private bool estabaEnDialogo;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            Time.timeScale = 1f; // por si se detuvo el Play estando en pausa
            EnPausa = false;
            panelPausa.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Lo llama el ícono de pausa (On Click del botón).</summary>
    public void Pausar()
    {
        // Se puede pausar en cualquier momento, incluso en medio de un
        // diálogo o leyendo una nota.
        if (EnPausa) return;

        EnPausa = true;
        Time.timeScale = 0f;

        estabaEnDialogo = GestorDialogo.EnDialogo;
        GestorDialogo.EnDialogo = true; // bloquea cámara y objetos del mundo

        botonPausa.SetActive(false);
        panelPausa.SetActive(true);
        InventoryManager.singleton.MostrarInterfaz(false);
    }

    /// <summary>Vuelve al juego. Lo usa la opción "Continuar".</summary>
    public void Reanudar()
    {
        if (!EnPausa) return;

        EnPausa = false;
        Time.timeScale = 1f;
        GestorDialogo.EnDialogo = estabaEnDialogo; // si había un diálogo, sigue ahí

        panelPausa.SetActive(false);
        botonPausa.SetActive(true);
        InventoryManager.singleton.MostrarInterfaz(true);
    }

    /// <summary>
    /// Muestra u oculta el ícono de pausa. Se usa en el menú principal,
    /// donde no tiene sentido poder pausar.
    /// </summary>
    public void MostrarIcono(bool visible)
    {
        botonPausa.SetActive(visible);
    }
}
