using UnityEngine;

/// <summary>
/// Colocá esto en cualquier escena para disparar un diálogo. Por
/// defecto arranca solo apenas empieza la escena, pero también podés
/// llamar a MostrarDialogo() manualmente desde otro script (por
/// ejemplo, el clic de un NPC) sin escribir una clase nueva por cada
/// diálogo del juego.
///
/// Si "soloUnaVez" está tildado, el diálogo queda registrado en
/// GestorEstadoJuego al mostrarse, así que no vuelve a aparecer
/// aunque la escena se recargue.
/// </summary>
public class DialogoTrigger : MonoBehaviour
{
    [Tooltip("Sprite del personaje que habla, se muestra durante todo el diálogo")]
    public Sprite personaje;

    [Tooltip("Las líneas del diálogo, en orden")]
    public LineaDialogo[] lineas;

    [Tooltip("Si está tildado, el diálogo arranca solo apenas empieza la escena")]
    public bool alIniciarLaEscena = true;

    [Header("Repetición")]
    [Tooltip("Si está tildado, este diálogo se muestra una sola vez en todo el juego, " +
             "aunque se recargue la escena")]
    public bool soloUnaVez = true;

    [Tooltip("Identificador ÚNICO de este diálogo (distinto para cada diálogo del juego). " +
             "Se usa para recordar si ya se mostró.")]
    public string idDialogo = "DialogoSinNombre";

    private void Start()
    {
        if (alIniciarLaEscena)
        {
            MostrarDialogo();
        }
    }

    public void MostrarDialogo()
    {
        if (soloUnaVez)
        {
            if (GestorEstadoJuego.singleton.YaOcurrio(idDialogo))
            {
                return; // Ya se mostró antes: no se repite
            }

            GestorEstadoJuego.singleton.MarcarComoOcurrido(idDialogo);
        }

        GestorDialogo.singleton.MostrarDialogo(personaje, lineas);
    }
}
