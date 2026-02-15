
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class InventoryRendering : MonoBehaviour
{
    private Queue<ItemRenderer> inventoryItems = new Queue<ItemRenderer>();
    private List<Slot> inventorySlots;
    [Inject]private CharackterModel characterModel;
    [SerializeField] private ItemRenderer prefab ;
    [SerializeField] private Transform parent ;
    private void Awake()
    {
        InitSlots();
        InitItems();
    }
    private void OnEnable()
    {
        foreach(InvenbtoryItem item in characterModel.Inventory)
        {
            var renderer = inventoryItems.Dequeue();
            renderer.Init(item);
            renderer.gameObject.SetActive(true);
            renderer.transform.SetParent(inventorySlots[item.SlotIndex].transform);
        }
    }
    void InitSlots()
    {
        inventorySlots = transform.GetComponentsInChildren<Slot>().ToList();
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].Init(i);
        }
    }
    void InitItems()
    {
       
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var item = Instantiate(prefab, inventorySlots[i].transform);
            inventoryItems.Enqueue(item);
            item.transform.SetParent(parent);
            item.gameObject.SetActive(false);
        }
    }
}
