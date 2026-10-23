using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Se asegura de que los gestores del juego (transición, inventario,
/// diálogos, música y estado) existan SIEMPRE, sin importar desde qué
/// escena se arranque. Unity ejecuta el método Arrancar() solo, antes de
/// que cargue la primera escena: no hay que colocar este script en
/// ningún objeto.
///
/// Los gestores viven todos dentro de un prefab llamado "Sistemas",
/// guardado en una carpeta "Resources". Así, tanto si se juega desde la
/// primera escena como si se prueba otra escena suelta, los gestores
/// ya están ahí y los demás scripts pueden usarlos normalmente.
///
/// Si una escena todavía trae su propia copia de algún gestor, esa
/// copia se destruye sola: cada gestor ya revisa en su Awake si existe
/// uno y, de ser así, destruye al duplicado.
/// </summary>
public static class ArranqueDeSistemas
{
    private const string NombreDelPrefab = "Sistemas";

    private static bool yaSeCargoUnaEscena;

    /// <summary>
    /// True si la escena que se está cargando es la primera de esta
    /// sesión de Play (es decir, la que se está probando directamente).
    /// Lo usa PruebaDeEscena para saber cuándo simular progreso.
    /// </summary>
    public static bool EsLaEscenaDeArranque => !yaSeCargoUnaEscena;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Arrancar()
    {
        yaSeCargoUnaEscena = false;

        // Se quita y se vuelve a poner para no suscribirse dos veces si
        // el Editor no recarga los scripts entre un Play y otro.
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;

        GameObject prefab = Resources.Load<GameObject>(NombreDelPrefab);

        if (prefab == null)
        {
            Debug.LogWarning("No se encontró el prefab '" + NombreDelPrefab + "' en una carpeta " +
                             "Resources. Los gestores no se van a cargar solos.");
            return;
        }

        GameObject sistemas = Object.Instantiate(prefab);
        sistemas.name = NombreDelPrefab;
        Object.DontDestroyOnLoad(sistemas); // todo lo que tiene adentro persiste entre escenas
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Este aviso llega DESPUÉS del Awake de la primera escena, así
        // que durante ese Awake EsLaEscenaDeArranque todavía es true.
        yaSeCargoUnaEscena = true;
    }
}
