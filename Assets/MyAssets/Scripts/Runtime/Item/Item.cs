using UnityEngine;

public class Item : MonoBehaviour
{
    private ItemData itemData;
    public ItemData ItemData => itemData;

    public void Initialize(ItemData data)
    {
        itemData = data;
        Instantiate(data.Prefab,transform);
    }

}
