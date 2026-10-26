using UnityEngine;

/// <summary>
/// Puente para controlar la cuenta regresiva desde los mini eventos del
/// Inspector de cualquier escena (igual que DisparadorDeMuerte).
///
/// ContadorRegresivo vive en "Sistemas", que no se puede arrastrar a un
/// UnityEvent. Este script sí va en un objeto de la escena:
///   - Iniciar() -> empieza la cuenta.
///   - Detener() -> la detiene.
/// </summary>
public class DisparadorDeContador : MonoBehaviour
{
    public void Iniciar()
    {
        ContadorRegresivo.singleton.Iniciar();
    }

    public void Detener()
    {
        ContadorRegresivo.singleton.Detener();
    }
}
