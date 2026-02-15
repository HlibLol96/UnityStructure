using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour
{
    private int _index;

    public Action<InvenbtoryItem> Droped;

    public void Init(int index)
    {
        _index = index;

    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        var itemRender = eventData.pointerDrag.GetComponent<ItemRenderer>();
        itemRender.transform.SetParent(transform);
        itemRender.transform.position = Vector2.zero;
        itemRender.Item.SlotIndex = _index;
    }
}
