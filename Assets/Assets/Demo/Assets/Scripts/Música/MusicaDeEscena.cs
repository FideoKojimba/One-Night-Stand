using UnityEngine;

/// <summary>
/// Colocá esto en cualquier escena/sala que deba sonar con una música
/// distinta a la general. Al entrar a esa escena, le pide al
/// GestorMusica que cambie de pista; si resulta ser la misma que ya
/// estaba sonando, GestorMusica simplemente la deja como está.
///
/// No hace falta usarlo en todas las escenas: si una sala no tiene
/// este componente, sigue sonando lo que ya estuviera sonando antes.
/// </summary>
public class MusicaDeEscena : MonoBehaviour
{
    [Tooltip("Música que debería sonar mientras el jugador está en esta escena")]
    public AudioClip musicaDeEstaSala;

    private void Start()
    {
        GestorMusica.singleton.ReproducirMusica(musicaDeEstaSala);
    }
}
