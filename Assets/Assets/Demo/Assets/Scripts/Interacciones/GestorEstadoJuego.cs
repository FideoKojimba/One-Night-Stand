using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registro persistente de "cosas que ya pasaron en el juego" y que no
/// deberían repetirse ni resetearse al recargar una escena — por
/// ejemplo, una puerta que ya se desbloqueó con una llave que ya se
/// gastó. Funciona con IDs de texto que vos elegís (por ejemplo,
/// "PuertaHospital"): es de uso general, no específico de un solo
/// objeto, así que sirve para cualquier cosa similar que agregues más
/// adelante (una palanca, un interruptor, un evento que solo debe
/// pasar una vez).
///
/// Persiste entre escenas (DontDestroyOnLoad), mismo patrón Singleton
/// que ya usan GestorMusica, GestorTransicionEscena e InventoryManager.
/// </summary>
public class GestorEstadoJuego : MonoBehaviour
{
    public static GestorEstadoJuego singleton;

    private HashSet<string> eventosOcurridos = new HashSet<string>();

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Marca este evento como ya ocurrido, de forma permanente.</summary>
    public void MarcarComoOcurrido(string idEvento)
    {
        eventosOcurridos.Add(idEvento);
    }

    /// <summary>Borra todo el registro: el juego vuelve a como estaba al empezar.</summary>
    public void Reiniciar()
    {
        eventosOcurridos.Clear();
    }

    /// <summary>Consulta si este evento ya ocurrió alguna vez antes.</summary>
    public bool YaOcurrio(string idEvento)
    {
        return eventosOcurridos.Contains(idEvento);
    }
}
