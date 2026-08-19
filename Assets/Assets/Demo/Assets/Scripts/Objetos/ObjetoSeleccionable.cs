using UnityEngine;

public class ObjetoSeleccionable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   private void OnMouseEnter()
    {
        AumentarEscala(true);
    }

   private void OnMouseExit()
    {
        AumentarEscala(false);
    }


    private Vector3 escalaInicial;

    private void Awake()
    {
        escalaInicial= transform.localScale;
    }

    private void AumentarEscala(bool status = true)
    {
        Vector3 escalaFinal = escalaInicial;

        if(status ==true)
        escalaFinal = escalaInicial * 1.1f;

        transform.localScale = escalaFinal;


}


public virtual void Examinar()
    {
        

    }
}
