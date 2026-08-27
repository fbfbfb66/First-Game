using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    [SerializeField] private SpriteRenderer icon;

    private void OnValidate()
    {
        if (itemData == null) return;
        gameObject.name = itemData.name;
        icon.sprite = itemData.Icon;
    }

    // 返回用于交互的位置，默认使用当前对象的 transform
    public Transform InteractionTransform => transform;

    // 是否允许交互：根据需要实现更复杂的判断逻辑
    public bool CanInteract(InteractionContext context)
    {
        if (itemData != null)
            return true;
        return false;
    }

    // 执行交互：根据游戏逻辑实现具体行为
    public void Interact(InteractionContext context)
    {
        var inventory = context.Interactor.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            if(inventory.TryAdd(itemData))
            {
                Destroy(gameObject);
                Debug.Log($"Picked up {itemData.name}");
            }
        }
    }

    // 返回交互提示文本
    public string GetInteractionPrompt(InteractionContext context)
    {
        return "F";
    }
}
