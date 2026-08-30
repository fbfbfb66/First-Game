using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    //宽和高是根据现在场景里的背包写的
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 8;

    public event Action<InventoryItem, int, int> ItemPlaced;
    public event Action<InventoryItem> ItemAmountUpdated;
    public event Action<InventoryItem> ItemRemoved;

    private InventoryGrid grid;

    /// <summary>
    /// 惰性创建：谁先访问谁负责建，之后所有人拿到同一个实例。
    /// 不能依赖 Awake —— Unity 只保证「自己的 Awake 在自己的 OnEnable 之前」，
    /// 跨对象的顺序由场景加载次序决定，InventoryView.OnEnable 完全可能跑在这之前。
    /// width / height 是序列化字段，Awake 之前就已完成反序列化，这里读取是安全的。
    /// </summary>
    private InventoryGrid Grid => grid ??= new InventoryGrid(width, height);

    public bool TryAdd(ItemData data, int amount = 1)
    {
        if (data == null || amount <= 0) return false;
        int over = amount - data.MaxStack;
        amount = Mathf.Min(amount, data.MaxStack);
        InventoryItem item = new InventoryItem(data, amount);
        InventoryItem stackable = Grid.FindStackable(data);
        if (stackable != null)
        {
            int overflow = stackable.Add(amount);
            ItemAmountUpdated?.Invoke(stackable);
            if (overflow == 0) return true;
            return TryAdd(data, overflow);
        }
        if (Grid.TryFindFreeCell(item, out int x, out int y))
        {
            Grid.Place(item, x, y);
            ItemPlaced?.Invoke(item, x, y);
            if(over > 0)
                return TryAdd(data, over);
            return true;
        }
        return false;
    }

    public bool TryRemove(InventoryItem item, int amount)
    {
        if (item == null || amount <= 0) return false;
        if(amount >= item.Amount)
        {
            if (Grid.Remove(item))
            {
                ItemRemoved?.Invoke(item);
                return true;
            }
        }
        else
        {
            item.Reduce(amount);
            ItemAmountUpdated?.Invoke(item);
            return true;
        }
        return false;
    }

    public bool TryMove(InventoryItem item, int x, int y,bool rotated)
    {
        if (item == null) return false;

        int width = rotated ? item.Data.Height : item.Data.Width;
        int height = rotated ? item.Data.Width : item.Data.Height;

        if (CanPlace(x,y,width,height,item))
        {
            if(item.IsRotated != rotated)
                item.Rotate();
            Grid.Remove(item);
            Grid.Place(item, x, y);
            return true;
        }
        return false;
    }
    public IEnumerable<(InventoryItem item, int x, int y)> GetPlacedItems() => Grid.GetPlacedItems();
    public bool IsInside(int x, int y) => Grid.IsInside(x, y);
    public bool CanPlace(InventoryItem item, int x, int y, bool ignoreItem = false) => Grid.CanPlace(item, x, y, ignoreItem);
    public bool CanPlace(int x,int y,int areaWidth,int areaHeight,InventoryItem ignoreItem = null) => Grid.CanPlace(x, y, areaWidth, areaHeight, ignoreItem);
    public InventoryItem GetItemAt(int x, int y) => Grid.GetItemAt(x, y);
}
