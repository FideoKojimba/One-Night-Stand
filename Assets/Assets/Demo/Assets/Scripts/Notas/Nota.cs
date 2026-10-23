using UnityEngine;

/// <summary>
/// Clase de DATOS pura: describe una nota. Igual que Item, no sabe
/// mostrarse ni recogerse; solo guarda su información. Quien la muestra
/// es GestorNotas, y quien la coloca en el mundo es WorldNota.
///
/// Cada página es una imagen (el PNG que hagas en Photoshop). Una nota
/// de una sola hoja tiene 1 página; una más larga, varias.
///
/// Está separada del mundo a propósito: más adelante un "inventario de
/// notas" podrá guardar objetos Nota sin cambiar esta clase.
/// </summary>
[System.Serializable]
public class Nota
{
    [Tooltip("Nombre único de la nota (sirve para identificarla en el futuro inventario de notas)")]
    public string nombre;

    [Tooltip("Las páginas en orden. Cada una es una imagen (Sprite).")]
    public Sprite[] paginas;
}
