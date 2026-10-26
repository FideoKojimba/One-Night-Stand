using System.Collections;
using UnityEngine;

/// <summary>
/// Reproduce un sonido y, MIENTRAS suena, impide que el jugador se vaya
/// a otra zona (las flechas y puertas que cambian de escena no
/// responden). Cuando el sonido termina, la salida se desbloquea sola.
///
/// Así el jugador escucha el sonido completo (por ejemplo, el de la
/// trampilla) y no se corta por cambiar de escena a mitad.
///
/// Solo bloquea las SALIDAS: la cámara, el inventario y los demás
/// objetos siguen funcionando con normalidad. Quien decide si puede
/// salir son los propios objetos de cambio de escena, que consultan
/// SonidoBloqueaSalida.SalidaBloqueada antes de reaccionar al clic.
///
/// Se usa desde los mini eventos del Inspector: arrastra este objeto y
/// elige SonidoBloqueaSalida > Reproducir(). Es genérico: sirve para
/// cualquier sonido que no deba cortarse.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SonidoBloqueaSalida : MonoBehaviour
{
    /// <summary>True mientras algún sonido de este tipo está sonando.</summary>
    public static bool SalidaBloqueada { get; private set; }

    [Tooltip("El sonido que se reproduce")]
    [SerializeField] private AudioClip sonido;

    [Range(0f, 1f)]
    [SerializeField] private float volumen = 1f;

    private AudioSource audioSource;
    private bool bloqueando; // true si ESTE objeto es quien tiene la salida bloqueada

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>Reproduce el sonido y bloquea las salidas hasta que termine.</summary>
    public void Reproducir()
    {
        if (sonido == null || bloqueando) return;

        StartCoroutine(SonarYBloquear());
    }

    private IEnumerator SonarYBloquear()
    {
        bloqueando = true;
        SalidaBloqueada = true;

        audioSource.PlayOneShot(sonido, volumen);

        // Realtime: cuenta bien incluso si el juego se pausa durante el sonido
        yield return new WaitForSecondsRealtime(sonido.length);

        Liberar();
    }

    // Seguridad: si este objeto se destruye con el sonido sonando
    // (por ejemplo, al reiniciar la partida), no se deja la salida
    // bloqueada para siempre.
    private void OnDestroy()
    {
        Liberar();
    }

    private void Liberar()
    {
        if (!bloqueando) return;

        bloqueando = false;
        SalidaBloqueada = false;
    }
}
