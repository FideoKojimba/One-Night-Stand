using UnityEngine;


public class ObjetoSeleccionable : MonoBehaviour
{
    public Sprite image;

   public void OnMouseEnter()
    {
        AumentarEscala(true);
    }

   public void OnMouseExit()
    {
        AumentarEscala(false);
    }


    public Vector2 escalaInicial;

    public void Awake()
    {
        escalaInicial= transform.localScale;
    }

    private void AumentarEscala(bool status = true)
    {
        Vector2 escalaFinal = escalaInicial;

        if(status ==true)
        escalaFinal = escalaInicial * 1.1f;

        transform.localScale = escalaFinal;


}


public virtual void Examinar()
    {
        

    }
}
