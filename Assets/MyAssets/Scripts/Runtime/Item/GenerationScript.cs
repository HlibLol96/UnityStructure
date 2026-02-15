using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GenerationScript : MonoBehaviour
{
    [SerializeField] List<ItemData> itemData;
    [SerializeField] Item item;
    ActionTimer timer;

    private void Awake()
    {
        timer = new ActionTimer(Generate,2);
    }

  

    private void Start()
    {
     timer.Start();

    }
    private void OnDisable()
    {
        timer.Stop();
    }
    async void Generate()
    { 
        await UniTask.SwitchToMainThread();
        var genItem = Instantiate(item, transform.position, Quaternion.identity,transform);
            genItem.Initialize(itemData[Random.Range(0, itemData.Count)]);
    } 
    
}
