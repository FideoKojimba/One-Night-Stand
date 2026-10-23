using UnityEngine;

/// <summary>
/// Clase de DATOS pura: una sola línea de diálogo. Igual que Item,
/// separa los datos de la lógica — GestorDialogo es quien sabe cómo
/// mostrarla, esta clase solo la describe.
/// </summary>
[System.Serializable]
public class LineaDialogo
{
    [Tooltip("Quién habla en esta línea (se muestra arriba de la caja). " +
             "Dejalo vacío si no querés mostrar nombre.")]
    public string nombre;

    [TextArea(2, 5)]
    public string texto;
}
