using System;
using UnityEngine;

public class InventoryItem 
{
    public ItemData Data { get; private set; }
    public int Amount { get; private set; }
    public bool IsRotated { get; private set; } = false;
    public int CurrentWidth => IsRotated ? Data.Height : Data.Width;
    public int CurrentHeight => IsRotated ? Data.Width : Data.Height;

    public InventoryItem(ItemData data, int amount)
    {
        if(data == null) 
            throw new ArgumentNullException(nameof(data));
        if(amount <= 0 || amount > data.MaxStack)
            throw new ArgumentOutOfRangeException(nameof(amount),amount, $"Amount must be between 1 and {data.MaxStack}");
        
        Data = data;
        Amount = amount;
    }

    public bool CanStackWith(ItemData data)
    {
        if(data != Data) return false;
        if(Amount >= Data.MaxStack) return false;
        return true;
    }

    public void Reduce(int amount)
    {
        if(amount <= 0 || amount > Amount)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amnount must be greater than 0 and less Amount");
        }
        Amount -= amount;
        Debug.Log($"Reduced {amount} to {Data.name}, new amount: {Amount}");
    }

    public int Add(int amount)
    {
        if(amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than 0");
        Amount += amount;
        Debug.Log($"Added {amount} to {Data.name}, new amount: {Amount}");
        if (Amount > Data.MaxStack)
        {
            int overflow = Amount - Data.MaxStack;
            Amount = Data.MaxStack;
            return overflow;
        }
        return 0;
    }

    public void Rotate()
    {
        if (Data.CanRotate == false) return;
        IsRotated = !IsRotated;
    }
}
