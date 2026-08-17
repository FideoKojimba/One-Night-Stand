using UnityEngine;

public class SistemadeClicks: MonoBehaviour
{
    [SerializeField] private Camera camaraPrincipal;
    [SerializeField] private float distanciaMaxima = 100f;

    private Objeto hoverActual;

    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
         DetectarObjetoBajoElCursor();

        if (Input.GetMouseButtonDown(0) && hoverActual != null)
        {
            hoverActual.Examinar();
        }
    }

    private void DetectarObjetoBajoElCursor()
    {

        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(rayo, out RaycastHit golpe, distanciaMaxima);
        
         Objeto nuevoHover;

        if (golpe.collider != null)
        {
            nuevoHover = golpe.collider.GetComponent<Objeto>();
        }
        else
        {
            nuevoHover = null;
        }

        if (nuevoHover == hoverActual) return;

        if (hoverActual != null) hoverActual.onMouseExit();
        hoverActual = nuevoHover;
        if (hoverActual != null) hoverActual.onMouseEnter();

    }
}
