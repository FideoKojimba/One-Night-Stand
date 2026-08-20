using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Slots[] inventorySlots;
    public GameObject inventoryItemPrefab;
   
   public bool AddItem(Items item)
    {
       
       for (int i = 0; i < inventorySlots.Length; i++)
        {
            Slots slot = inventorySlots[i];
            IconoInventario inventoryItem = slot.GetComponentInChildren<IconoInventario>();
            if (inventoryItem == null)
            {
                SpawnItem(item, slot);
                return true;
            }
           
        }
        return false;
    }

    void SpawnItem(Items item, Slots slot)
    {
       GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        IconoInventario inventoryItem = newItemGo.GetComponent<IconoInventario>();
        inventoryItem.InitialiseItem(item);
    }
}
