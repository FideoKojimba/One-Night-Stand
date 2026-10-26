using UnityEngine;

/// <summary>
/// Reproduce el sonido de pasos del personaje. Responsabilidad única:
/// elegir un sonido y reproducirlo. NO sabe CUÁNDO se camina; eso lo
/// decide quien lo llama (GestorTransicionEscena, al empezar el efecto
/// de caminado).
///
/// Si le das varios sonidos, elige uno al azar cada vez, y le cambia
/// un poco el tono, para que los pasos no suenen idénticos siempre.
/// Si le das uno solo, suena ese.
///
/// Va en un objeto hijo de "Sistemas" para que persista entre escenas:
/// así el sonido no se corta cuando la escena cambia en medio del paso.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SonidoPasos : MonoBehaviour
{
    [Tooltip("Uno o varios sonidos de pasos. Si hay varios, se elige uno al azar.")]
    [SerializeField] private AudioClip[] sonidosDePasos;

    [Header("Variación de tono (para que no suene siempre igual)")]
    [Tooltip("Tono mínimo. 1 = normal")]
    [SerializeField] private float tonoMinimo = 0.95f;
    [Tooltip("Tono máximo. 1 = normal")]
    [SerializeField] private float tonoMaximo = 1.05f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>Reproduce un sonido de pasos (al azar entre los disponibles).</summary>
    public void Reproducir()
    {
        if (sonidosDePasos == null || sonidosDePasos.Length == 0) return;

        AudioClip elegido = sonidosDePasos[Random.Range(0, sonidosDePasos.Length)];
        audioSource.pitch = Random.Range(tonoMinimo, tonoMaximo);
        audioSource.PlayOneShot(elegido);
    }
}
