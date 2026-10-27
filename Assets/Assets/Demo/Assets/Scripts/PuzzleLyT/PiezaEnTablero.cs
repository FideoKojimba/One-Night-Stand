using UnityEngine;

/// <summary>
/// Un gadget colocado sobre el tablero. Lo crea PuzzleDeRanuras por
/// código, así no necesitas armar un prefab por cada gadget.
///
/// Hereda de ObjetoSeleccionable para tener el mismo hover (agrandarse)
/// que el resto de objetos. Con el mouse se puede:
///  - hacer CLIC (sin mover): vuelve al inventario.
///  - ARRASTRAR y soltar cerca de otra ranura libre: se encaja ahí.
///    Si se suelta lejos de cualquiera, regresa a donde estaba.
///
/// Cuando el puzzle se resuelve, PuzzleDeRanuras la bloquea: ya no se
/// puede mover ni guardar.
/// </summary>
public class PiezaEnTablero : ObjetoSeleccionable
{
    // Cuántos píxeles debe moverse el mouse para considerarse un arrastre y no un clic
    private const float umbralDeArrastre = 10f;

    public PiezaDePuzzle Datos { get; private set; }

    private PuzzleDeRanuras puzzle;
    private RanuraPuzzle ranura;
    private SpriteRenderer sprite;
    private Collider2D colision;

    private bool bloqueada;
    private bool presionada;
    private bool arrastrando;
    private Vector2 posicionPantallaInicial;
    private Vector3 desfaseConElMouse;
    private int ordenOriginal;

    public void Inicializar(PuzzleDeRanuras puzzleDuenio, PiezaDePuzzle datos, RanuraPuzzle ranuraInicial)
    {
        puzzle = puzzleDuenio;
        Datos = datos;
        ranura = ranuraInicial;
        sprite = GetComponent<SpriteRenderer>();
        colision = GetComponent<Collider2D>();
        ordenOriginal = sprite.sortingOrder;
    }

    /// <summary>La pieza queda fija para siempre (puzzle resuelto).</summary>
    public void Bloquear()
    {
        bloqueada = true;
        colision.enabled = false;
    }

    private void OnMouseDown()
    {
        if (bloqueada || GestorDialogo.EnDialogo) return;
        if (InventoryManager.PunteroSobreInventario()) return; // clic sobre el inventario: no agarrar lo de atrás

        presionada = true;
        arrastrando = false;
        posicionPantallaInicial = Input.mousePosition;
        desfaseConElMouse = transform.position - PosicionDelMouseEnElMundo();
    }

    private void OnMouseDrag()
    {
        if (!presionada) return;

        // Hasta que el mouse no se mueva lo suficiente, sigue siendo un clic
        if (!arrastrando && Vector2.Distance(Input.mousePosition, posicionPantallaInicial) > umbralDeArrastre)
        {
            arrastrando = true;
            sprite.sortingOrder = ordenOriginal + 20; // pasa por encima de todo mientras se arrastra
        }

        if (arrastrando)
        {
            transform.position = PosicionDelMouseEnElMundo() + desfaseConElMouse;
        }
    }

    private void OnMouseUp()
    {
        if (!presionada) return;
        presionada = false;

        if (arrastrando)
        {
            arrastrando = false;
            Soltar();
        }
        else
        {
            Guardar();
        }
    }

    // Clic sin arrastrar: la pieza vuelve al inventario (si hay espacio)
    private void Guardar()
    {
        if (InventoryManager.singleton.AgregarItem(Datos.item))
        {
            puzzle.QuitarPieza(this, ranura);
        }
        // Si el inventario está lleno, AgregarItem devuelve false y la pieza se queda donde está
    }

    // Termina el arrastre: se encaja en la ranura libre más cercana, o vuelve atrás
    private void Soltar()
    {
        sprite.sortingOrder = ordenOriginal;

        RanuraPuzzle destino = puzzle.RanuraLibreBajo(transform.position, this);

        if (destino == null || destino == ranura)
        {
            ranura.Asignar(this); // vuelve a su sitio
            return;
        }

        ranura.Liberar();
        destino.Asignar(this);
        ranura = destino;

        puzzle.ReproducirSonidoColocar();
        puzzle.AlCambiarEstado();
    }

    private Vector3 PosicionDelMouseEnElMundo()
    {
        Vector3 punto = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        punto.z = transform.position.z;
        return punto;
    }

    private void OnMouseEnter() { IniciarHover(); }
    private void OnMouseExit() { TerminarHover(); }
}
