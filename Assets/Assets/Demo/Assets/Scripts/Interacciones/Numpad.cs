using System.Collections;
using UnityEngine;

/// <summary>
/// Maneja un puzzle de numpad SIN feedback visual: el jugador escribe
/// el código directamente con el TECLADO (no hay botones en pantalla
/// para clickear), guiándose solo por el sonido.
///
/// Cada dígito presionado suena un "beep"; al completar la cantidad
/// de dígitos del código, se compara automáticamente (no hace falta
/// confirmar con Enter ni nada) y suena un acierto o un error.
///
/// Los sonidos nunca se superponen: se usa un único AudioSource, y
/// antes de reproducir el resultado (acierto/error) se espera a que
/// termine de sonar el beep del último dígito.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Numpad : MonoBehaviour
{
    [Header("Puzzle")]
    [Tooltip("El código que el jugador debe ingresar para resolverlo")]
    public string codigoCorrecto = "4036";

    [Tooltip("Identificador único de este puzzle (el mismo id que usa la PuertaConCodigo correspondiente)")]
    public string idPuzzle = "PuzzleSinNombre";

    [Header("Qué pasa al acertar")]
    [Tooltip("Escena a la que se vuelve automáticamente al ingresar el código correcto")]
    public string escenaDeRegreso;

    [Header("Sonidos")]
    [Tooltip("Se reproduce cada vez que se presiona CUALQUIER dígito")]
    public AudioClip sonidoDigito;
    [Tooltip("Se reproduce si el código ingresado es correcto")]
    public AudioClip sonidoCorrecto;
    [Tooltip("Se reproduce si el código ingresado es incorrecto")]
    public AudioClip sonidoIncorrecto;

    private AudioSource audioSource;
    private string digitosIngresados = "";
    private bool procesandoResultado = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Mientras se está verificando el código (esperando a que termine
        // de sonar el beep antes del resultado), o mientras hay un diálogo
        // en pantalla, se ignora el teclado.
        if (procesandoResultado || GestorDialogo.EnDialogo)
        {
            return;
        }

        // Revisa las teclas 0-9 tanto de la fila superior (Alpha) como
        // del teclado numérico (Keypad), para que funcione con cualquiera.
        for (int digito = 0; digito <= 9; digito++)
        {
            KeyCode teclaFilaSuperior = KeyCode.Alpha0 + digito;
            KeyCode teclaNumerica = KeyCode.Keypad0 + digito;

            if (Input.GetKeyDown(teclaFilaSuperior) || Input.GetKeyDown(teclaNumerica))
            {
                AgregarDigito(digito.ToString());
                break; // ya se procesó un dígito en este frame, no revisar el resto
            }
        }
    }

    private void AgregarDigito(string digito)
    {
        digitosIngresados += digito;
        ReproducirSonido(sonidoDigito);

        if (digitosIngresados.Length >= codigoCorrecto.Length)
        {
            procesandoResultado = true;
            StartCoroutine(VerificarCodigoAlTerminarElBeep());
        }
    }

    private IEnumerator VerificarCodigoAlTerminarElBeep()
    {
        // Espera a que termine de sonar el beep del último dígito antes
        // de reproducir el sonido de acierto o de error, para que no se
        // superpongan.
        yield return new WaitWhile(() => audioSource.isPlaying);

        if (digitosIngresados == codigoCorrecto)
        {
            ReproducirSonido(sonidoCorrecto);
            GestorEstadoJuego.singleton.MarcarComoOcurrido(idPuzzle);

            // También esperamos a que termine el sonido de acierto antes
            // de cambiar de escena, para no cortarlo de golpe.
            yield return new WaitWhile(() => audioSource.isPlaying);

            GestorTransicionEscena.singleton.IrAEscena(escenaDeRegreso, transform.position, false);
        }
        else
        {
            ReproducirSonido(sonidoIncorrecto);
            digitosIngresados = "";
            yield return new WaitWhile(() => audioSource.isPlaying);
            procesandoResultado = false;
        }
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
