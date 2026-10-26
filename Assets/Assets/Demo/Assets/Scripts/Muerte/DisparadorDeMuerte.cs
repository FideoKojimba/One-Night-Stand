using UnityEngine;

/// <summary>
/// Puente para activar la muerte de Blair desde los mini eventos del
/// Inspector de cualquier escena.
///
/// Por qué existe: GestorMuerte vive dentro del prefab "Sistemas", que
/// no está en la escena y por eso no se puede arrastrar a un UnityEvent.
/// Este script SÍ puede ponerse en un objeto de la escena: conecta el
/// evento con "DisparadorDeMuerte > Morir()" y este llama al gestor.
///
/// Se puede usar en cualquier lugar (una trampa, un objeto, una puerta,
/// el resultado de un ObjetoReceptorDeItem...) sin escribir código nuevo.
/// </summary>
public class DisparadorDeMuerte : MonoBehaviour
{
    public void Morir()
    {
        GestorMuerte.singleton.Morir();
    }
}
