using UnityEngine;

/// <summary>
/// Da sensación de profundidad: cuando la cámara se mueve (por ejemplo
/// con CamaraMirarBordes al acercar el mouse a un borde), este objeto
/// se desplaza un poco MENOS o un poco MÁS que la cámara, según qué
/// tan "cerca" o "lejos" del jugador quieras que se sienta.
///
/// No sabe nada de CÓMO se mueve la cámara — podría ser
/// CamaraMirarBordes, un futuro efecto de caminado, o cualquier otra
/// cosa. Solo compara la posición de la cámara contra la del frame
/// anterior y reacciona a la diferencia. Responsabilidad única: mover
/// este objeto según el movimiento de la cámara.
/// </summary>
public class EfectoParallax : MonoBehaviour
{
    [Tooltip(
        "0 = el objeto no se mueve NUNCA (se siente muy lejano, como una pared de fondo).\n" +
        "1 = el objeto se mueve EXACTAMENTE igual que la cámara (se siente pegado a la pantalla, muy cercano).\n" +
        "Valores intermedios (0.3 a 0.8) son los más útiles para casi todo lo demás: " +
        "cuanto más alto, más 'cerca del jugador' se percibe el objeto."
    )]
    [Range(0f, 1f)]
    public float factorParallax = 0.5f;

    private Transform camara;
    private Vector3 posicionCamaraAnterior;

    private void Start()
    {
        camara = Camera.main.transform;
        posicionCamaraAnterior = camara.position;
    }

    // LateUpdate corre DESPUÉS de que CamaraMirarBordes ya movió la
    // cámara en este mismo frame, así siempre lee la posición final.
    private void LateUpdate()
    {
        Vector3 movimientoCamara = camara.position - posicionCamaraAnterior;
        transform.position += movimientoCamara * factorParallax;
        posicionCamaraAnterior = camara.position;
    }
}
