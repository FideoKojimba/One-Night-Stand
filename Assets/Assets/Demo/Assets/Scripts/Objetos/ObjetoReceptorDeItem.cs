using System.Collections;
using UnityEngine;

/// <summary>
/// Objeto genérico que acepta un ítem específico arrastrado desde el
/// inventario (por ejemplo, una llave, una bolsa de sangre, una pieza
/// que falta en una máquina). Al aceptarlo: cambia su propio sprite,
/// espera un momento, y dispara la transición (fundido a negro) hacia
/// otra escena.
///
/// Reutiliza IObjetoInteractivo — la misma interfaz que ya usa
/// PuertaConLlave — y GestorTransicionEscena para el fundido y el
/// cambio de escena. Pensado para reusarse en cualquier objeto del
/// juego que siga este mismo patrón: "recibe esto, se transforma,
/// avanza a otro lado", sin tener que escribir una clase nueva cada
/// vez.
/// </summary>
public class ObjetoReceptorDeItem : MonoBehaviour, IObjetoInteractivo
{
    [Tooltip("Debe coincidir exactamente con el nombre del Item que este objeto acepta")]
    public string nombreItemRequerido = "Item Requerido";

    [Header("Qué pasa al aceptarlo")]
    [Tooltip("Sprite al que cambia este mismo objeto al aceptar el ítem")]
    public Sprite spriteAlAceptar;

    [Tooltip("Cuánto espera (en segundos) después de cambiar el sprite, antes de fundir a negro")]
    public float tiempoDeEspera = 1.5f;

    [Tooltip("Escena a la que se llega después del fundido")]
    public string escenaDestino;

    [Tooltip("Si querés el efecto de caminado al ir hacia la escena destino")]
    public bool usarEfectoCaminado = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool UsarItem(Item item)
    {
        if (item.nombre != nombreItemRequerido)
        {
            return false; // No es el ítem correcto: se rechaza
        }

        StartCoroutine(SecuenciaDeAceptacion());
        return true; // Aceptado: el ítem se consume
    }

    private IEnumerator SecuenciaDeAceptacion()
    {
        if (spriteAlAceptar != null)
        {
            spriteRenderer.sprite = spriteAlAceptar;
        }

        yield return new WaitForSeconds(tiempoDeEspera);

        GestorTransicionEscena.singleton.IrAEscena(escenaDestino, transform.position, usarEfectoCaminado);
    }
}
