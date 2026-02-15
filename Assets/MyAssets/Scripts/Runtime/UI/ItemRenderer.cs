using System;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemRenderer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private TextMeshProUGUI _fastNumber;
   

    public event Action<ItemRenderer> ItemDropped;
    public InvenbtoryItem Item { get; private set; }

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Init(InvenbtoryItem item)
    {
        Item = item;
        _icon.sprite = item.ItemData.Icon;
        _icon.enabled = true;
        _countText.text = item.Quantity > 1 ? item.Quantity.ToString() : string.Empty;


        if (_rectTransform)
            _rectTransform.SetStretch();

    }

    public void Clear()
    {
        Item = null;
        _icon.sprite = null;
        _countText.text = string.Empty;
        _icon.enabled = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = GetComponentInParent<Slot>();
        slot.transform.SetAsLastSibling();
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localPosition = Vector2.zero;
        _canvasGroup.blocksRaycasts = true;

        
       
    }
}