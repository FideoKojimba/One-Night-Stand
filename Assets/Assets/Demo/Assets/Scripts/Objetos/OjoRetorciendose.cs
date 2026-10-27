using UnityEngine;

/// <summary>
/// Hace que un objeto (el ojo de Blair) se retuerza: tiembla, gira y
/// late un poco, de forma irregular (ruido de Perlin, no un vaivén
/// mecánico). Tiene DOS "estados" de intensidad:
///  - normal    : un retorcimiento suave (al empezar la cinemática).
///  - intenso   : rápido y violento. Se activa llamando a Intensificar(),
///                que se conecta desde el Inspector como cualquier mini evento
///                (por ejemplo, desde MostrarSiOcurrioEvento > Al Entrar Con
///                Evento Ocurrido).
///
/// Todos los valores se ajustan en el Inspector, sin tocar código.
/// Parte siempre de la posición, el giro y la escala que el objeto tenga
/// al empezar, así que basta con colocarlo sobre la cuenca del ojo.
/// </summary>
public class OjoRetorciendose : MonoBehaviour
{
    [Header("Retorcimiento suave (estado normal)")]
    [Tooltip("Qué tan rápido cambia el movimiento")]
    [SerializeField] private float velocidad = 3f;
    [Tooltip("Cuánto se desplaza, en unidades del mundo (valores chicos: 0.01 - 0.05)")]
    [SerializeField] private float desplazamiento = 0.03f;
    [Tooltip("Cuánto gira, en grados")]
    [SerializeField] private float giro = 6f;
    [Tooltip("Cuánto late (cambia de tamaño). 0.03 = 3%")]
    [SerializeField] private float latido = 0.02f;

    [Header("Retorcimiento violento (después de Intensificar)")]
    [SerializeField] private float velocidadIntensa = 14f;
    [SerializeField] private float desplazamientoIntenso = 0.12f;
    [SerializeField] private float giroIntenso = 25f;
    [SerializeField] private float latidoIntenso = 0.08f;

    private Vector3 posicionBase;
    private float giroBase;
    private Vector3 escalaBase;
    private float semilla;      // desfasa el ruido para que no sea siempre igual
    private bool intenso;

    private void Awake()
    {
        posicionBase = transform.localPosition;
        giroBase = transform.localEulerAngles.z;
        escalaBase = transform.localScale;
        semilla = Random.Range(0f, 100f);
    }

    /// <summary>Pasa al retorcimiento violento (mini evento para el Inspector).</summary>
    public void Intensificar()
    {
        intenso = true;
    }

    private void Update()
    {
        float vel = intenso ? velocidadIntensa : velocidad;
        float desp = intenso ? desplazamientoIntenso : desplazamiento;
        float gir = intenso ? giroIntenso : giro;
        float lat = intenso ? latidoIntenso : latido;

        float t = Time.time * vel + semilla;

        // Ruido de Perlin va de 0 a 1: se pasa a -1..1 para poder ir a ambos lados
        float x = Ruido(t, 0f);
        float y = Ruido(t, 10f);
        float a = Ruido(t, 20f);
        float e = Ruido(t, 30f);

        transform.localPosition = posicionBase + new Vector3(x, y, 0f) * desp;
        transform.localRotation = Quaternion.Euler(0f, 0f, giroBase + a * gir);
        transform.localScale = escalaBase * (1f + e * lat);
    }

    private float Ruido(float t, float desfase)
    {
        return (Mathf.PerlinNoise(t, desfase) - 0.5f) * 2f;
    }
}
