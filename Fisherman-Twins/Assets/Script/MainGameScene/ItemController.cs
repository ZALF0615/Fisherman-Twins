using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public List<Item> passiveItems = new List<Item>();
    
    private void Start()
    {
        
    }

    private void Update()
    {
        foreach(Item item in passiveItems)
        {
            item.timeLeft-=Time.deltaTime;
            if(item.timeLeft<=0)
            {
                passiveItems.Remove(item);
                break;
            }
        }
    }

    public void TryAdd(int idx, bool isPassive, float duration)
    {
        foreach(Item item in passiveItems)
        {
            if(item.idx==idx) passiveItems.Remove(item);
            break;
        }
        passiveItems.Add(new Item(idx, isPassive, duration));
        foreach(Item item in passiveItems)
        {
            Debug.Log(item);
        }
    }
}

public class Item
{
    public int idx;
    public bool isPassive;
    public float timeLeft;

    public Item(int idx, bool isPassive, float timeLeft)
    {
        this.idx = idx;
        this.isPassive = isPassive;
        this.timeLeft = timeLeft;
    }
    public override string ToString()
    {
        if (isPassive) return $"itemIdx = {idx} / timeLeft = {timeLeft}s";
        else return $"itemIdx = {idx}";
    }
}