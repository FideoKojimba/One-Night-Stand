using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Coordina un puzzle de gadgets sobre ranuras: el jugador arrastra
/// gadgets del inventario a las ranuras del tablero, y cuando cada
/// ranura de la solución tiene su gadget correcto, el puzzle se resuelve.
///
/// Reparto de responsabilidades (una clase, una tarea):
///  - PiezaDePuzzle  : datos de cada gadget (Item + sprite en el tablero).
///  - RanuraPuzzle   : cada hueco; sabe qué gadget le corresponde.
///  - PiezaEnTablero : el gadget ya colocado; se puede guardar o mover.
///  - PuzzleDeRanuras: esta clase; crea las piezas, recuerda el estado,
///                     detecta la solución y lanza la secuencia final.
///
/// Memoria entre escenas: cada vez que algo cambia, se guarda en
/// GestorEstadoJuego qué gadget hay en cada ranura. Al volver a la
/// escena, todo queda exactamente como se dejó.
///
/// Al resolverse (en orden): sonido de colocar, sonido de "correcto",
/// sonido del cofre, mini eventos opcionales y diálogo final. Durante
/// la secuencia (sonidos y diálogo) el jugador no puede irse de la escena;
/// antes de resolver el puzzle sí puede entrar y salir cuando quiera. Además
/// deja registrado el evento "idPuzzle" para que OTRA escena reaccione
/// (por ejemplo, un cofre que aparece abierto, usando
/// MostrarSiOcurrioEvento con ese mismo id).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PuzzleDeRanuras : MonoBehaviour
{
    [Header("Identificación")]
    [Tooltip("Identificador ÚNICO de este puzzle. También es el id del evento " +
             "'puzzle resuelto' que otras escenas pueden consultar.")]
    [SerializeField] private string idPuzzle = "PuzzleSinNombre";

    [Header("Contenido")]
    [Tooltip("Todos los gadgets que se pueden colocar en este tablero (incluida la calavera)")]
    [SerializeField] private PiezaDePuzzle[] piezas;
    [Tooltip("Las ranuras del tablero")]
    [SerializeField] private RanuraPuzzle[] ranuras;

    [Header("Cómo se ven las piezas colocadas")]
    [Tooltip("Sorting Layer de las piezas (la misma que usan tus objetos interactivos)")]
    [SerializeField] private string capaDeOrden = "Interactivos";
    [Tooltip("Orden dentro de esa capa (más alto = se dibuja encima del tablero)")]
    [SerializeField] private int ordenDeLasPiezas = 5;
    [Tooltip("Escala con la que se ven las piezas. Ajústala hasta que calcen con las ranuras.")]
    [SerializeField] private float escalaDeLasPiezas = 1f;

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoColocar;
    [SerializeField] private AudioClip sonidoCorrecto;
    [SerializeField] private AudioClip sonidoCofre;

    [Header("Al resolverse")]
    [Tooltip("Nombres de ítems que se tiran del inventario al resolver (ej. 'Calavera')")]
    [SerializeField] private string[] itemsADescartar;
    [Tooltip("Otros mini eventos (después de los sonidos y antes del diálogo)")]
    [SerializeField] private UnityEvent alResolver;
    [Tooltip("(Opcional) Diálogo de Blair al final. Debe tener 'Al Iniciar La Escena' desmarcado.")]
    [SerializeField] private DialogoTrigger dialogoFinal;

    private AudioSource audioSource;

    /// <summary>True cuando el puzzle ya se resolvió (queda así para siempre).</summary>
    public bool Resuelto { get; private set; }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        Resuelto = GestorEstadoJuego.singleton.YaOcurrio(idPuzzle);

        foreach (RanuraPuzzle ranura in ranuras)
        {
            ranura.Inicializar(this);

            // Lo guardado manda; si nunca se guardó nada, vale el estado inicial
            string guardado = GestorEstadoJuego.singleton.ObtenerValor(Clave(ranura));
            if (guardado == null) guardado = ranura.NombreItemInicial;

            if (!string.IsNullOrEmpty(guardado))
            {
                PiezaDePuzzle datos = BuscarPieza(guardado);
                if (datos != null)
                {
                    CrearPieza(datos, ranura, false);
                }
            }
        }

        GuardarEstado();
    }

    // ---------- Consultas que usan las ranuras y las piezas ----------

    /// <summary>Busca los datos del gadget con este nombre (null si no es de este puzzle).</summary>
    public PiezaDePuzzle BuscarPieza(string nombreItem)
    {
        foreach (PiezaDePuzzle pieza in piezas)
        {
            if (pieza.item != null && pieza.item.nombre == nombreItem)
            {
                return pieza;
            }
        }
        return null;
    }

    /// <summary>
    /// La ranura libre sobre la que está el CENTRO de la pieza, o null si
    /// no está sobre ninguna. Es el encaje "estricto": la pieza solo se
    /// encaja si se suelta dentro del área (collider) de la ranura.
    /// </summary>
    public RanuraPuzzle RanuraLibreBajo(Vector3 posicion, PiezaEnTablero quienPregunta)
    {
        foreach (RanuraPuzzle ranura in ranuras)
        {
            bool libre = !ranura.Ocupada || ranura.Pieza == quienPregunta;
            if (libre && ranura.Contiene(posicion))
            {
                return ranura;
            }
        }
        return null;
    }

    // ---------- Crear y quitar piezas ----------

    /// <summary>Crea por código un gadget encajado en la ranura indicada.</summary>
    public void CrearPieza(PiezaDePuzzle datos, RanuraPuzzle ranura, bool conSonido)
    {
        GameObject objeto = new GameObject("Pieza " + datos.item.nombre);

        // La pieza queda un poquito MÁS CERCA de la cámara que la ranura (z menor).
        // Así, al hacer clic, Unity elige la pieza y no el collider de la ranura que tiene debajo.
        Vector3 posicion = ranura.transform.position;
        posicion.z -= 0.1f;
        objeto.transform.position = posicion;

        // La escala se fija ANTES de agregar PiezaEnTablero: así el hover
        // (que recuerda la escala inicial) crece desde la escala correcta.
        objeto.transform.localScale = Vector3.one * escalaDeLasPiezas;

        SpriteRenderer sr = objeto.AddComponent<SpriteRenderer>();
        sr.sprite = datos.spriteEnTablero;
        sr.sortingLayerName = capaDeOrden;
        sr.sortingOrder = ordenDeLasPiezas;

        // El área de clic de la pieza es IGUAL al área de su ranura: lo que ves
        // dentro de la ranura es lo que puedes agarrar. (No se usa la forma del sprite:
        // el arte con puntitos/dither deja una forma rota y difícil de clicar.)
        BoxCollider2D caja = objeto.AddComponent<BoxCollider2D>();
        Collider2D areaRanura = ranura.Colision;
        if (areaRanura != null)
        {
            caja.size = areaRanura.bounds.size / escalaDeLasPiezas;
            caja.offset = (areaRanura.bounds.center - ranura.transform.position) / escalaDeLasPiezas;
        }
        else
        {
            caja.size = datos.spriteEnTablero.bounds.size;
        }

        PiezaEnTablero pieza = objeto.AddComponent<PiezaEnTablero>();
        pieza.Inicializar(this, datos, ranura);
        ranura.Asignar(pieza);

        if (Resuelto) pieza.Bloquear();
        if (conSonido) ReproducirSonidoColocar();
    }

    /// <summary>Saca una pieza del tablero (porque volvió al inventario).</summary>
    public void QuitarPieza(PiezaEnTablero pieza, RanuraPuzzle ranura)
    {
        ranura.Liberar();
        Destroy(pieza.gameObject);

        ReproducirSonidoColocar();
        AlCambiarEstado();
    }

    public void ReproducirSonidoColocar()
    {
        if (sonidoColocar != null) audioSource.PlayOneShot(sonidoColocar);
    }

    // ---------- Estado y solución ----------

    /// <summary>Se llama cada vez que una pieza se coloca, se mueve o se guarda.</summary>
    public void AlCambiarEstado()
    {
        GuardarEstado();

        if (!Resuelto && EstaResuelto())
        {
            StartCoroutine(SecuenciaDeResolucion());
        }
    }

    private bool EstaResuelto()
    {
        bool hayRanurasRequeridas = false;

        foreach (RanuraPuzzle ranura in ranuras)
        {
            if (ranura.EsRequerida) hayRanurasRequeridas = true;
            if (!ranura.EstaCorrecta) return false;
        }

        return hayRanurasRequeridas;
    }

    private void GuardarEstado()
    {
        foreach (RanuraPuzzle ranura in ranuras)
        {
            string contenido = ranura.Ocupada ? ranura.Pieza.Datos.item.nombre : "";
            GestorEstadoJuego.singleton.GuardarValor(Clave(ranura), contenido);
        }
    }

    private string Clave(RanuraPuzzle ranura)
    {
        return idPuzzle + "/" + ranura.Id;
    }

    // ---------- Secuencia final ----------

    private IEnumerator SecuenciaDeResolucion()
    {
        // Queda registrado para siempre (también lo lee la otra escena, ej. el cofre)
        Resuelto = true;
        GestorEstadoJuego.singleton.MarcarComoOcurrido(idPuzzle);

        // Las piezas quedan fijas
        foreach (RanuraPuzzle ranura in ranuras)
        {
            if (ranura.Ocupada) ranura.Pieza.Bloquear();
        }

        // Un frame de espera: deja terminar el arrastre del último gadget
        yield return null;

        // Desde aquí hasta que termine el diálogo final, el jugador no puede
        // interactuar ni irse de la escena (EnDialogo bloquea las salidas).
        // Antes de este punto (puzzle sin resolver) puede entrar y salir libremente.
        GestorDialogo.EnDialogo = true;

        foreach (string nombre in itemsADescartar)
        {
            InventoryManager.singleton.DescartarItem(nombre);
        }

        // Cada sonido espera a que termine el anterior (tiempo real: sigue
        // contando bien aunque el jugador pause el juego).
        // (el sonido de colocar ya sonó al soltar la pieza: solo se espera a que termine)
        yield return new WaitForSecondsRealtime(sonidoColocar != null ? sonidoColocar.length : 0f);
        yield return Reproducir(sonidoCorrecto);
        yield return Reproducir(sonidoCofre);

        alResolver.Invoke();

        if (dialogoFinal != null)
        {
            dialogoFinal.MostrarDialogo(LiberarJuego);
        }
        else
        {
            LiberarJuego();
        }
    }

    private WaitForSecondsRealtime Reproducir(AudioClip clip)
    {
        if (clip == null) return new WaitForSecondsRealtime(0f);

        audioSource.PlayOneShot(clip);
        return new WaitForSecondsRealtime(clip.length);
    }

    private void LiberarJuego()
    {
        GestorDialogo.EnDialogo = false;
    }
}
