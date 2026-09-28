using UnityEngine;

/// <summary>
/// Clase de DATOS pura: una sola línea de diálogo. Igual que Item,
/// separa los datos de la lógica — GestorDialogo es quien sabe cómo
/// mostrarla, esta clase solo la describe.
/// </summary>
[System.Serializable]
public class LineaDialogo
{
    [TextArea(2, 5)]
    public string texto;
}
