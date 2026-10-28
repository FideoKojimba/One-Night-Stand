using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Una nota tirada en el mundo. Es el equivalente a WorldItem, pero en
/// vez de ir al inventario, abre la nota a pantalla completa.
///
/// Hereda de ObjetoSeleccionable para tener el mismo hover (agrandarse)
/// que el resto de objetos.
///
/// Mientras la nota está abierta, el objeto del suelo se oculta (porque
/// "viaja" a la pantalla). Al cerrarla, la animación termina justo donde
/// está este objeto y entonces reaparece.
/// </summary>
public class WorldNota : ObjetoSeleccionable
{
    public Nota nota;

    [Header("Diálogo antes de agarrar la nota (opcional)")]
    [Tooltip("Si lo asignas, Blair habla PRIMERO y la nota se abre cuando el diálogo termina. " +
             "Debe ser un objeto con DialogoTrigger (con 'Al Iniciar La Escena' desmarcado). " +
             "Si el diálogo es 'Solo Una Vez' y ya se dijo, la nota se abre directo.")]
    public DialogoTrigger dialogoAntesDeLeer;

    [Header("Mini eventos")]
    [Tooltip("Qué ocurre justo cuando la nota SE ABRE (al recogerla). Ej.: un sonido, " +
             "Screamer > Iniciar() para un susto mientras la lee.")]
    public UnityEvent alAbrirNota;

    [Tooltip("Qué ocurre en el INSTANTE en que el jugador pulsa la flecha para cerrar la nota " +
             "(antes de la animación de cierre). Ej.: Screamer > Asustar().")]
    public UnityEvent alPulsarCerrar;

    [Tooltip("Qué ocurre cuando el jugador CIERRA la nota (con la flecha) y esta termina de " +
             "volver al suelo. Ej.: DialogoTrigger > MostrarDialogo para que Blair comente la nota.")]
    public UnityEvent alCerrarNota;

    private SpriteRenderer sprite;
    private Collider2D colision;
    private Vector3 escalaInicial;

    protected override void Awake()
    {
        base.Awake();
        sprite = GetComponent<SpriteRenderer>();
        colision = GetComponent<Collider2D>();
        escalaInicial = transform.localScale;
    }

    private void OnMouseDown()
    {
        if (GestorDialogo.EnDialogo) return;
        if (InventoryManager.PunteroSobreInventario()) return; // clic sobre el inventario: no agarrar lo de atrás

        if (dialogoAntesDeLeer != null)
        {
            // Primero habla Blair; al terminar, se abre la nota
            dialogoAntesDeLeer.MostrarDialogo(AbrirNota);
        }
        else
        {
            AbrirNota();
        }
    }

    private void AbrirNota()
    {
        // Punto de la pantalla donde está la nota (de ahí sale la animación)
        Vector2 origen = Camera.main.WorldToScreenPoint(transform.position);

        // Se oculta el objeto del suelo y se evita que reciba clics
        Ocultar();
        transform.localScale = escalaInicial; // evita quedar agrandada por el hover

        GestorNotas.singleton.Abrir(nota, origen, Mostrar, alPulsarCerrar.Invoke);

        alAbrirNota.Invoke();
    }

    // Se llama cuando la nota terminó de volver al suelo
    private void Mostrar()
    {
        sprite.enabled = true;
        colision.enabled = true;

        alCerrarNota.Invoke();
    }

    private void Ocultar()
    {
        sprite.enabled = false;
        colision.enabled = false;
    }

    private void OnMouseEnter() { IniciarHover(); }
    private void OnMouseExit() { TerminarHover(); }
}
