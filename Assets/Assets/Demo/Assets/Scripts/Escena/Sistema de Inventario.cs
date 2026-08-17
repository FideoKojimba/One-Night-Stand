using UnityEngine;
using UnityEngine.UI;

public class SistemadeInventario : MonoBehaviour
{
    
    public static SistemadeInventario Instancia;

    public GameObject panelInventario;


    
    public int objetosRecogidos = 0;

    private bool inventarioAbierto = false;





   private void Start()
    {
       panelInventario.SetActive(false);

       for (int i = 0; i < panelInventario.transform.childCount; i++)
        {
            Image cuadro = panelInventario.transform.GetChild(i).GetComponent<Image>();
            if (cuadro != null)
            {
                cuadro.sprite = null;
                cuadro.enabled = false;
            }
        }
    }


   

   public void AlternarInventario()
    {
        if (inventarioAbierto == true)
        {
            inventarioAbierto = false;
        }
        else
        {
            inventarioAbierto = true;
        }

        if (panelInventario != null)
        {
            panelInventario.SetActive(inventarioAbierto);
        }
    }

    
}
