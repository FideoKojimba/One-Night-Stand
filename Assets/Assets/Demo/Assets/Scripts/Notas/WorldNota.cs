using UnityEngine;

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

        // Punto de la pantalla donde está la nota (de ahí sale la animación)
        Vector2 origen = Camera.main.WorldToScreenPoint(transform.position);

        // Se oculta el objeto del suelo y se evita que reciba clics
        Ocultar();
        transform.localScale = escalaInicial; // evita quedar agrandada por el hover

        GestorNotas.singleton.Abrir(nota, origen, Mostrar);
    }

    // Se llama cuando la nota terminó de volver al suelo
    private void Mostrar()
    {
        sprite.enabled = true;
        colision.enabled = true;
    }

    private void Ocultar()
    {
        sprite.enabled = false;
        colision.enabled = false;
    }

    private void OnMouseEnter() { IniciarHover(); }
    private void OnMouseExit() { TerminarHover(); }
}
