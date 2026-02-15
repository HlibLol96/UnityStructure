using UnityEditor.Build;

public class InvenbtoryItem 
{
    public ItemData ItemData { get; set; }
    public int Quantity { get; set; }
    public int SlotIndex {  get; set; }

    public bool IsFullstack => Quantity >= stackAmount;

    private int stackAmount = 4;

    public InvenbtoryItem(ItemData itemData, int quantity)
    {
        ItemData = itemData;
        Quantity = quantity;
        
    }
    public void AddItem(int quantity)
    {
        Quantity += quantity;
    }   
}
