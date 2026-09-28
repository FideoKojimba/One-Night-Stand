using UnityEngine;

/// <summary>
/// Se coloca sobre la Main Camera. Su única responsabilidad es mover
/// la cámara cuando el mouse se acerca a un borde de la pantalla, para
/// "revelar" más fondo hacia ese lado (típico de juegos point & click).
///
/// No sabe nada del inventario, de los ítems ni de los objetos
/// interactivos: solo lee la posición del mouse y mueve su propio
/// transform dentro de unos límites que vos configurás desde el
/// Inspector. Esto respeta el principio de responsabilidad única (una
/// clase, un trabajo) igual que las demás clases del proyecto.
/// </summary>
public class CamaraMirarBordes : MonoBehaviour
{
    [Header("Sensibilidad")]
    [Tooltip("Qué tan rápido se mueve la cámara al detectar el borde")]
    public float velocidadDesplazamiento = 5f;

    [Tooltip("Distancia en píxeles desde el borde de la pantalla que activa el movimiento")]
    public float margenBorde = 60f;

    [Header("Límites del fondo")]
    [Tooltip("Punto más lejano (abajo-izquierda) al que puede llegar la cámara")]
    public Vector2 limiteMinimo;

    [Tooltip("Punto más lejano (arriba-derecha) al que puede llegar la cámara")]
    public Vector2 limiteMaximo;

    private void Update()
    {
        Vector3 direccion = CalcularDireccion();

        if (direccion != Vector3.zero)
        {
            Vector3 posicionDeseada = transform.position + direccion * velocidadDesplazamiento * Time.deltaTime;
            transform.position = ClampearDentroDelFondo(posicionDeseada);
        }
    }

    /// <summary>
    /// Revisa en qué borde de la pantalla está el mouse ahora mismo y
    /// devuelve hacia dónde debería moverse la cámara. Puede combinar
    /// dos ejes a la vez (si el mouse está en una esquina, se mueve en
    /// diagonal).
    /// </summary>
    private Vector3 CalcularDireccion()
    {
        Vector3 posicionMouse = Input.mousePosition;
        Vector3 direccion = Vector3.zero;

        if (posicionMouse.x <= margenBorde)
        {
            direccion.x = -1f;
        }
        else if (posicionMouse.x >= Screen.width - margenBorde)
        {
            direccion.x = 1f;
        }

        if (posicionMouse.y <= margenBorde)
        {
            direccion.y = -1f;
        }
        else if (posicionMouse.y >= Screen.height - margenBorde)
        {
            direccion.y = 1f;
        }

        return direccion;
    }

    /// <summary>
    /// Evita que la cámara se salga del fondo y muestre el vacío que
    /// hay detrás de él (o de otra escena/objeto que no debería verse).
    /// </summary>
    private Vector3 ClampearDentroDelFondo(Vector3 posicion)
    {
        posicion.x = Mathf.Clamp(posicion.x, limiteMinimo.x, limiteMaximo.x);
        posicion.y = Mathf.Clamp(posicion.y, limiteMinimo.y, limiteMaximo.y);
        return posicion;
    }
}
