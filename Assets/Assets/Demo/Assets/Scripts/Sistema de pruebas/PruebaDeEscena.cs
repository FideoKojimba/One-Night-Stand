using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Herramienta para PROBAR una escena suelta: simula el progreso que
/// normalmente llevarías si hubieras jugado desde el principio (puertas
/// ya abiertas, puzzles resueltos, ítems en el inventario).
///
/// Solo actúa en el Editor de Unity, y solo cuando apretás Play estando
/// justo en la escena que tiene este componente. Si jugás normalmente
/// desde la primera escena, o en un build, no hace absolutamente nada,
/// así que se puede dejar puesto sin riesgo.
///
/// [DefaultExecutionOrder(-100)] hace que su Awake corra antes que el de
/// los demás scripts de la escena. Es necesario porque, por ejemplo,
/// PuertaConLlave revisa en su propio Awake si ya estaba abierta.
/// </summary>
[DefaultExecutionOrder(-100)]
public class PruebaDeEscena : MonoBehaviour
{
    [Tooltip("Ids que se dan por ya ocurridos. Son los mismos que escribiste en " +
             "PuertaConLlave (Id Puerta), PuertaConCodigo / Numpad (Id Puzzle) " +
             "y DialogoTrigger (Id Dialogo).")]
    public List<string> eventosYaOcurridos = new List<string>();

    [Tooltip("Ítems con los que arranca el inventario. El Nombre tiene que ser " +
             "exactamente el mismo que usa la puerta que lo pide.")]
    public List<Item> itemsEnInventario = new List<Item>();

    private void Awake()
    {
#if UNITY_EDITOR
        if (!ArranqueDeSistemas.EsLaEscenaDeArranque)
        {
            return; // se llegó a esta escena jugando: el progreso es el real
        }

        foreach (string id in eventosYaOcurridos)
        {
            GestorEstadoJuego.singleton.MarcarComoOcurrido(id);
        }

        foreach (Item item in itemsEnInventario)
        {
            InventoryManager.singleton.AgregarItem(item);
        }
#endif
    }
}
