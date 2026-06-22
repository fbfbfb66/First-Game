using System;
public class InventoryItem 
{
    public ItemData Data { get; private set; }
    public int Amount { get; private set; }
    public bool IsRotated { get; private set; }
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

    public void Rotate()
    {
        if(!Data.CanRotate) return;
        IsRotated = !IsRotated;
    }
}
