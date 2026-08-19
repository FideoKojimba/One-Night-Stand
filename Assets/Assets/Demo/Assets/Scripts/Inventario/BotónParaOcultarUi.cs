using UnityEngine;

public class BotónParaOcultarUi : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Inventario;
    
    public void Trigger()
    {
        if(Inventario.activeInHierarchy == false)
        {
            Inventario.SetActive(true);
        }

        else

        {
            Inventario.SetActive(false);
        }
    }

}
