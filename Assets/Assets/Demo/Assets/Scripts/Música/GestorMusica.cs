using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor de música de fondo. Vive en el prefab "Sistemas" (persiste
/// entre escenas) y es el ÚNICO que toca el AudioSource de la música.
///
/// Qué hace:
///  - Reproduce una pista en bucle (música de fondo).
///  - Si una escena pide la pista que ya está sonando, no la corta:
///    sigue sin interrupciones.
///  - Cuando cambia de pista, recuerda en qué segundo se quedó la
///    anterior. Si más tarde esa pista vuelve a pedirse, quien la pide
///    decide si continúa desde ese segundo o empieza desde cero.
///
/// Quién decide qué música suena en cada escena NO es esta clase: eso
/// lo configura cada escena con MusicaDeEscena (responsabilidad única).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class GestorMusica : MonoBehaviour
{
    public static GestorMusica singleton;

    private AudioSource audioSource;

    // Mientras sea true, la música de las escenas (MusicaDeEscena) se
    // ignora: suena una canción "forzada" que interrumpe a todas las demás.
    private bool musicaForzada;

    // Memoria: para cada pista, el segundo donde quedó al cambiar de música.
    private Dictionary<AudioClip, float> posicionesGuardadas = new Dictionary<AudioClip, float>();

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;          // música de fondo = bucle
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Pide que suene una pista.
    /// continuarDondeQuedo = true  -> si esa pista ya sonó antes, retoma
    ///                                desde el segundo donde se detuvo.
    /// continuarDondeQuedo = false -> empieza desde el principio.
    /// Si la pista ya está sonando ahora mismo, no hace nada (sigue igual).
    /// </summary>
    public void ReproducirMusica(AudioClip nuevaMusica, bool continuarDondeQuedo)
    {
        if (nuevaMusica == null) return;

        // Hay una canción forzada sonando: ninguna escena puede cambiarla
        if (musicaForzada) return;

        // Ya está sonando: no se reinicia ni se corta.
        if (audioSource.clip == nuevaMusica && audioSource.isPlaying) return;

        // Antes de cambiar, guardamos dónde iba la pista actual.
        GuardarPosicionActual();

        audioSource.clip = nuevaMusica;

        float inicio = 0f;
        if (continuarDondeQuedo && posicionesGuardadas.ContainsKey(nuevaMusica))
        {
            inicio = posicionesGuardadas[nuevaMusica];

            // Seguridad: si el valor quedó fuera de la duración, empieza de cero.
            if (inicio >= nuevaMusica.length) inicio = 0f;
        }

        audioSource.time = inicio;
        audioSource.Play();
    }

    /// <summary>
    /// Interrumpe lo que esté sonando y pone esta canción, desde el
    /// principio. Mientras esté forzada, las escenas NO pueden cambiar la
    /// música (se ignoran sus MusicaDeEscena). Se libera con
    /// LiberarMusicaForzada(). Se usa, por ejemplo, para la canción del
    /// último minuto de la cuenta regresiva.
    /// </summary>
    public void ForzarMusica(AudioClip musica)
    {
        if (musica == null) return;

        GuardarPosicionActual(); // la canción interrumpida recuerda dónde iba

        musicaForzada = true;
        audioSource.clip = musica;
        audioSource.time = 0f;
        audioSource.Play();
    }

    /// <summary>
    /// Vuelve a permitir que las escenas cambien la música. La canción
    /// forzada sigue sonando hasta que alguna escena pida otra.
    /// </summary>
    public void LiberarMusicaForzada()
    {
        musicaForzada = false;
    }

    /// <summary>Guarda el segundo actual de la pista que está sonando.</summary>
    private void GuardarPosicionActual()
    {
        if (audioSource.clip != null)
        {
            posicionesGuardadas[audioSource.clip] = audioSource.time;
        }
    }

    /// <summary>
    /// Olvida todas las posiciones guardadas y detiene la música.
    /// Útil para el botón de reiniciar juego.
    /// </summary>
    public void Reiniciar()
    {
        posicionesGuardadas.Clear();
        musicaForzada = false;
        audioSource.Stop();
        audioSource.clip = null;
    }
}
