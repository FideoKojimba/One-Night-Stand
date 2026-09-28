using UnityEngine;
using System.Collections;

/// <summary>
/// Clase base ("padre") para cualquier objeto que el jugador pueda
/// seleccionar con el mouse: un ítem en el mundo, un ícono del
/// inventario, o lo que se agregue más adelante.
///
/// Esta clase SOLO sabe animar el tamaño del objeto al entrar/salir
/// del hover. No sabe nada sobre CÓMO se detecta ese hover, porque eso
/// cambia según el tipo de objeto (un objeto del mundo usa colisiones
/// físicas; un elemento de UI usa el sistema de eventos de Unity).
/// Cada clase hija llama a IniciarHover()/TerminarHover() desde su
/// propio mecanismo de detección.
/// </summary>
public abstract class ObjetoSeleccionable : MonoBehaviour
{
    [Header("Animación de hover")]
    [SerializeField] private float multiplicadorEscala = 1.1f;
    [SerializeField] private float duracionAnimacion = 0.1f;

    private Vector3 escalaOriginal;
    private Coroutine animacionActual;

    protected virtual void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    /// <summary>Llamar cuando el mouse empieza a pasar por encima.</summary>
    protected void IniciarHover()
    {
        if (GestorDialogo.EnDialogo) return;

        AnimarHacia(escalaOriginal * multiplicadorEscala);
    }

    /// <summary>Llamar cuando el mouse deja de estar encima.</summary>
    protected void TerminarHover()
    {
        if (GestorDialogo.EnDialogo) return;

        AnimarHacia(escalaOriginal);
    }

    private void AnimarHacia(Vector3 escalaDestino)
    {
        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
        }

        animacionActual = StartCoroutine(AnimarEscala(escalaDestino));
    }

    private IEnumerator AnimarEscala(Vector3 escalaDestino)
    {
        Vector3 escalaInicial = transform.localScale;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionAnimacion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = tiempoTranscurrido / duracionAnimacion;
            transform.localScale = Vector3.Lerp(escalaInicial, escalaDestino, progreso);
            yield return null;
        }

        transform.localScale = escalaDestino; // asegura que termine exacto
    }
}
