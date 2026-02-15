using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [field : SerializeField] public string Name { get; private set; }
    [field : SerializeField] public Sprite Icon { get; private set; }
    [field : SerializeField] public GameObject Prefab { get; private set; }
    
}
