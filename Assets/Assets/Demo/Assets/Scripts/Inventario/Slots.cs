using UnityEngine;
using UnityEngine.EventSystems;

public class Slots : MonoBehaviour, IDropHandler
{
    public void OnDrop (PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            IconoInventario iconoInventario = eventData.pointerDrag.GetComponent<IconoInventario>();
            iconoInventario.parentAfterDrag = transform;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
