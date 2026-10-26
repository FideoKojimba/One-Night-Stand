using UnityEngine;

/// <summary>
/// Opción del menú de pausa: abandona la partida y vuelve al menú
/// principal.
///
/// Como el juego todavía no tiene sistema de guardado, la partida se
/// REINICIA por completo (inventario, puertas abiertas, música). Así,
/// al elegir "Comenzar" otra vez, empieza desde cero y no arrastra
/// el progreso anterior.
/// </summary>
public class OpcionVolverAlMenu : OpcionMenu
{
    [Header("Destino")]
    [Tooltip("Nombre EXACTO de la escena del menú principal (debe estar en Build Settings)")]
    [SerializeField] private string escenaMenu;

    protected override void Ejecutar()
    {
        // Primero se sale de la pausa: si no, el tiempo seguiría
        // congelado y el fundido a negro nunca avanzaría.
        GestorPausa.singleton.Reanudar();

        // Se cierran de golpe un diálogo o una nota que estuvieran abiertos
        GestorDialogo.singleton.Reiniciar();
        GestorNotas.singleton.Reiniciar();

        if (ContadorRegresivo.singleton != null) ContadorRegresivo.singleton.Reiniciar();
        InventoryManager.singleton.Reiniciar();
        GestorEstadoJuego.singleton.Reiniciar();
        GestorMusica.singleton.Reiniciar();

        GestorTransicionEscena.singleton.IrAEscena(escenaMenu, transform.position, false);
    }
}
