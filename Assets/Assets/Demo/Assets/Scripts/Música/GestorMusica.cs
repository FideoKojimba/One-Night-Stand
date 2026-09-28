using UnityEngine;

/// <summary>
/// Único punto de control de la música de fondo. Persiste entre
/// escenas (mismo patrón que GestorTransicionEscena e InventoryManager),
/// así la música no se corta ni reinicia con cada cambio de escena.
///
/// Por ahora reproduce siempre la misma pista (musicaInicial), pero ya
/// queda preparado para tener música distinta por sala más adelante:
/// cualquier otro script puede llamar a ReproducirMusica(clip) con una
/// pista diferente (por ejemplo, desde MusicaDeEscena) y este gestor
/// se encarga de cambiarla — sin reiniciarla si ya es la que está sonando.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GestorMusica : MonoBehaviour
{
    public static GestorMusica singleton;

    [Tooltip("Música que empieza a sonar apenas arranca el juego")]
    public AudioClip musicaInicial;

    private AudioSource audioSource;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
        else
        {
            // Ya existe uno persistiendo de una escena anterior: este
            // duplicado se destruye, igual que hacen InventoryManager
            // y GestorTransicionEscena con los suyos.
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (musicaInicial != null)
        {
            ReproducirMusica(musicaInicial);
        }
    }

    /// <summary>
    /// Cambia la pista que suena. Si ya es la misma que está sonando
    /// ahora mismo, no hace nada — así, si volvés a entrar a una sala
    /// que ya visitaste, la música sigue sonando sin cortarse ni
    /// reiniciar desde el principio.
    /// </summary>
    public void ReproducirMusica(AudioClip nuevaMusica)
    {
        if (nuevaMusica == null || audioSource.clip == nuevaMusica)
        {
            return;
        }

        audioSource.clip = nuevaMusica;
        audioSource.Play();
    }
}
