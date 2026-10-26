/// <summary>
/// Opción del menú de pausa: cierra la pausa y sigue jugando.
/// El hover y los sonidos los hereda de OpcionMenu.
/// </summary>
public class OpcionContinuar : OpcionMenu
{
    protected override void Ejecutar()
    {
        GestorPausa.singleton.Reanudar();
    }
}
