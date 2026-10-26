using UnityEngine;

/// <summary>
/// Clase de DATOS pura (igual que Item y Nota): describe un gadget del
/// puzzle de ranuras. Une dos cosas:
///  - el Item que vive en el inventario (nombre, ícono...), y
///  - el sprite con el que se ve cuando está colocado sobre el tablero.
///
/// No hace nada por sí sola: PuzzleDeRanuras la usa como una "tabla" de
/// todos los gadgets posibles del puzzle.
/// </summary>
[System.Serializable]
public class PiezaDePuzzle
{
    [Tooltip("Los mismos datos que tiene el Item de este gadget en su WorldItem " +
             "(el Nombre debe ser IDÉNTICO).")]
    public Item item;

    [Tooltip("Cómo se ve el gadget cuando está colocado sobre el tablero")]
    public Sprite spriteEnTablero;
}
