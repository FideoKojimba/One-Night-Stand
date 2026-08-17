using UnityEngine;
using UnityEngine.UI;

public class ObjetodeInventario : Objeto
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Examinar()
    {
        SistemadeInventario sis = SistemadeInventario.Instancia;

       if (sis != null && sis.objetosRecogidos < sis.panelInventario.transform.childCount)
        {
            Transform cuadro = sis.panelInventario.transform.GetChild(sis.objetosRecogidos);
            Image imagenUI = cuadro.GetComponent<Image>();

            if (imagenUI != null)
            {
                imagenUI.sprite = iconoInventario;
                imagenUI.enabled = true;
            }

            sis.objetosRecogidos++; 
            
            Debug.Log($"{nombre} tomó el control y se posicionó en el cuadro {sis.objetosRecogidos}");
            
            
            gameObject.SetActive(false); 
        }
        else
        {
            Debug.Log("El inventario está lleno.");
        }
    }
}
