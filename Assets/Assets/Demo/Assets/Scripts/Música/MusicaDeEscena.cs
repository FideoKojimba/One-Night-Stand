using UnityEngine;

/// <summary>
/// Se coloca en un objeto de CADA escena y le dice al GestorMusica qué
/// hacer con la música al entrar. Solo configura; no reproduce nada por
/// sí misma.
///
/// Tres casos posibles:
///  1) cambiarMusica = false -> la escena no toca la música: sigue la
///     que venía sonando (o silencio si no había).
///  2) cambiarMusica = true, continuarDondeQuedo = true -> suena la
///     pista de esta escena retomando donde se quedó la última vez
///     (ej.: minuto 2:30). Si es la primera vez, empieza de cero.
///  3) cambiarMusica = true, continuarDondeQuedo = false -> suena la
///     pista de esta escena siempre desde el principio.
/// </summary>
public class MusicaDeEscena : MonoBehaviour
{
    [Tooltip("¿Esta escena cambia la música? Si está apagado, se mantiene la que ya suena.")]
    public bool cambiarMusica = true;

    [Tooltip("Pista que suena en esta escena (solo se usa si 'Cambiar Musica' está activado).")]
    public AudioClip musicaDeEstaSala;

    [Tooltip("Activado: retoma la pista donde se quedó (ej. minuto 2:30). " +
             "Desactivado: empieza desde el principio cada vez.")]
    public bool continuarDondeQuedo = true;

    private void Start()
    {
        if (!cambiarMusica) return;

        GestorMusica.singleton.ReproducirMusica(musicaDeEstaSala, continuarDondeQuedo);
    }
}
