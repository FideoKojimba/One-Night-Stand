using UnityEngine;

/// <summary>
/// Una ranura (hueco) del tablero del puzzle. Implementa
/// IObjetoInteractivo, la misma interfaz que PuertaConLlave: así el
/// jugador puede arrastrar un gadget desde el inventario hasta ella
/// y el sistema de arrastre ya existente la reconoce sin cambios.
///
/// Una ranura solo sabe tres cosas: qué gadget la resuelve
/// (nombreItemCorrecto), si ya hay una pieza puesta, y aceptar o
/// rechazar lo que le llega del inventario. Quien coordina todo el
/// puzzle es PuzzleDeRanuras.
///
/// Necesita un Collider2D en el mismo objeto (para recibir el arrastre).
/// </summary>
public class RanuraPuzzle : MonoBehaviour, IObjetoInteractivo
{
    [Tooltip("Identificador ÚNICO de esta ranura dentro del puzzle (ej. 'AbajoDerecha'). " +
             "Se usa para recordar qué gadget hay puesto aunque se cambie de escena.")]
    [SerializeField] private string idRanura = "Ranura";

    [Tooltip("Nombre del Item que debe ir en esta ranura para resolver el puzzle. " +
             "Déjalo VACÍO si esta ranura no forma parte de la solución.")]
    [SerializeField] private string nombreItemCorrecto = "";

    [Tooltip("(Opcional) Nombre de un Item que ya está puesto en esta ranura al empezar el juego " +
             "(ej. la calavera, que se puede agarrar desde el tablero).")]
    [SerializeField] private string nombreItemInicial = "";

    private PuzzleDeRanuras puzzle;
    private Collider2D colision;

    public string Id => idRanura;
    public string NombreItemInicial => nombreItemInicial;
    public PiezaEnTablero Pieza { get; private set; }
    public bool Ocupada => Pieza != null;

    /// <summary>True si esta ranura ya está bien: no es parte de la solución, o tiene su gadget correcto.</summary>
    public bool EstaCorrecta
    {
        get
        {
            if (string.IsNullOrEmpty(nombreItemCorrecto)) return true;
            return Ocupada && Pieza.Datos.item.nombre == nombreItemCorrecto;
        }
    }

    /// <summary>True si esta ranura forma parte de la solución.</summary>
    public bool EsRequerida => !string.IsNullOrEmpty(nombreItemCorrecto);

    /// <summary>El collider de esta ranura (define el área donde se encaja una pieza).</summary>
    public Collider2D Colision
    {
        get
        {
            if (colision == null) colision = GetComponent<Collider2D>();
            return colision;
        }
    }

    /// <summary>True si este punto del mundo cae dentro del área de la ranura.</summary>
    public bool Contiene(Vector3 punto)
    {
        return Colision != null && Colision.OverlapPoint(punto);
    }

    public void Inicializar(PuzzleDeRanuras puzzleDuenio)
    {
        puzzle = puzzleDuenio;
    }

    /// <summary>Un gadget llegó arrastrado desde el inventario.</summary>
    public bool UsarItem(Item item)
    {
        // Si el puzzle ya está resuelto o la ranura está ocupada, se rechaza
        if (puzzle == null || puzzle.Resuelto || Ocupada) return false;

        PiezaDePuzzle datos = puzzle.BuscarPieza(item.nombre);
        if (datos == null) return false; // no es un gadget de este puzzle

        puzzle.CrearPieza(datos, this, true);
        puzzle.AlCambiarEstado();
        return true; // aceptado: el ítem sale del inventario
    }

    /// <summary>Deja esta pieza puesta en la ranura, encajada en su centro.</summary>
    public void Asignar(PiezaEnTablero pieza)
    {
        Pieza = pieza;

        Vector3 posicion = transform.position;
        posicion.z = pieza.transform.position.z; // se conserva la profundidad de la pieza
        pieza.transform.position = posicion;
    }

    /// <summary>La ranura queda vacía (la pieza se fue o se movió a otra).</summary>
    public void Liberar()
    {
        Pieza = null;
    }
}
