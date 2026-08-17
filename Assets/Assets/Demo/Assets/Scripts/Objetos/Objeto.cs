using UnityEngine;

public class Objeto : MonoBehaviour
{
    public string nombre;
    public string descripción;
    public Sprite iconoInventario;

    [SerializeField] private Color colorOriginal;
   [SerializeField] private Color tinteRojizo = new Color(1f, 0.5f, 0.5f);

     private Renderer renderizador;

     protected virtual void Awake()
    {
        renderizador = GetComponent<Renderer>();
        colorOriginal = renderizador.material.color;
    }

    public void onMouseEnter()
    {
        renderizador.material.color = colorOriginal * tinteRojizo;
    }

    public void onMouseExit()
    {
       renderizador.material.color = colorOriginal;
    }   


    public virtual void Examinar()
    {
        
    }
}
