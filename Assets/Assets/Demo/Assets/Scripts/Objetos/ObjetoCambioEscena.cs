using System.Collections;
using UnityEngine;

/// <summary>
/// Cualquier objeto que, al hacer clic, cambie de escena (una flecha,
/// una puerta de salida, un portal, etc.) puede usar este mismo
/// componente. Cada instancia solo necesita indicar a qué escena
/// lleva desde el Inspector; no hace falta escribir un script nuevo
/// por cada flecha que agregues al juego.
///
/// Hereda de ObjetoSeleccionable para tener la misma animación de
/// hover que el resto de los objetos seleccionables.
///
/// Opcionalmente puede estar "bloqueado" hasta que ocurra un evento del
/// juego (por ejemplo, un cofre que solo se abre después de resolver un
/// puzzle). Mientras el evento no haya ocurrido, en vez de cambiar de
/// escena muestra un diálogo (por ejemplo, "no puedo abrirlo").
/// </summary>
public class ObjetoCambioEscena : ObjetoSeleccionable
{
    [Tooltip("Debe coincidir EXACTAMENTE con el nombre de la escena. " +
             "Esa escena también debe estar agregada en File > Build Settings.")]
    public string nombreEscena;

    [Tooltip("Desmarcá esto si el fondo de destino calza justo con la " +
             "pantalla (sin margen de sobra): el acercamiento de cámara " +
             "revelaría un borde que no debería verse. El fundido a " +
             "negro se hace siempre, esto solo afecta el 'paso' previo.")]
    public bool usarEfectoCaminado = true;

    [Header("Bloqueo hasta que ocurra un evento (opcional)")]
    [Tooltip("Id del evento que debe haber ocurrido para poder pasar (ej. el Id Puzzle de un " +
             "PuzzleDeRanuras). Déjalo VACÍO si este objeto siempre debe funcionar.")]
    public string idEventoRequerido = "";

    [Tooltip("(Opcional) Sonido que suena PRIMERO al intentar pasar estando bloqueado " +
             "(ej. un cofre que no abre). Al terminar, sale el diálogo.")]
    public AudioClip sonidoBloqueado;

    [Tooltip("Diálogo que se muestra mientras el evento NO haya ocurrido. Debe tener " +
             "'Al Iniciar La Escena' desmarcado y 'Solo Una Vez' desmarcado, para que se " +
             "repita cada vez que el jugador lo intente.")]
    public DialogoTrigger dialogoSiEstaBloqueado;

    private AudioSource audioSource;
    private bool reproduciendoBloqueo; // evita repetir el aviso si hacen clic varias veces seguidas

    protected override void Awake()
    {
        base.Awake(); // guarda la escala original para el hover

        // Fuente de audio propia, solo si se va a usar el sonido de bloqueo
        if (sonidoBloqueado != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnMouseDown()
    {
        if (GestorDialogo.EnDialogo) return;
        if (SonidoBloqueaSalida.SalidaBloqueada) return; // hay un sonido importante sonando: no se puede salir aún

        // Si hace falta un evento y todavía no ocurrió, se muestra el diálogo en vez de pasar
        if (!string.IsNullOrEmpty(idEventoRequerido) &&
            !GestorEstadoJuego.singleton.YaOcurrio(idEventoRequerido))
        {
            if (!reproduciendoBloqueo)
            {
                StartCoroutine(AvisoDeBloqueo());
            }
            return;
        }

        GestorTransicionEscena.singleton.IrAEscena(nombreEscena, transform.position, usarEfectoCaminado);
    }

    // Primero suena el aviso (si hay), y recién al terminar sale el diálogo
    private IEnumerator AvisoDeBloqueo()
    {
        reproduciendoBloqueo = true;

        if (sonidoBloqueado != null)
        {
            audioSource.PlayOneShot(sonidoBloqueado);
            yield return new WaitForSecondsRealtime(sonidoBloqueado.length);
        }

        if (dialogoSiEstaBloqueado != null)
        {
            dialogoSiEstaBloqueado.MostrarDialogo();
        }

        reproduciendoBloqueo = false;
    }

    private void OnMouseEnter()
    {
        IniciarHover();
    }

    private void OnMouseExit()
    {
        TerminarHover();
    }
}
