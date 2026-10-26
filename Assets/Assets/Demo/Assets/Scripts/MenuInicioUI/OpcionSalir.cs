using UnityEngine;

/// <summary>
/// Opción de menú que cierra el juego. Solo define QUÉ hace al
/// confirmarse; el hover y los sonidos los hereda de OpcionMenu.
///
/// Nota: Application.Quit() no hace nada dentro del editor de Unity ni
/// en WebGL (el navegador no deja cerrar la pestaña). Por eso, en el
/// editor se detiene el modo Play para poder probarlo.
/// </summary>
public class OpcionSalir : OpcionMenu
{
    protected override void Ejecutar()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
