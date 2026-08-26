using UnityEngine;

/// <summary>
/// Clase de DATOS pura: describe qué es un ítem, pero no sabe
/// nada sobre cómo se agrega, se muestra o se quita del inventario.
/// Separar los "datos" de la "lógica" es un principio básico de POO
/// (una clase, una responsabilidad).
/// </summary>
[System.Serializable]
public class Item
{
    public string nombre;
    public Sprite icono;
    public GameObject prefab; // Lo que aparece en el mundo si se suelta el ítem
}
