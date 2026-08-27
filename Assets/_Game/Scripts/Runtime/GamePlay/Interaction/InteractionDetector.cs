using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private GameObject interactor;
    private readonly List<IInteractable> interactions = new();
    private IInteractable current;
    private InteractionPrompt currentPrompt;

    private void Awake()
    {
        interactor = transform.root.gameObject;
    }

    private void Update()
    {
        // 被销毁的对象不会立刻从列表里消失：OnTriggerExit2D 跟着物理更新走，
        // 而 Update 每帧都在跑，中间至少隔着一帧。那一帧里遍历到死引用就会抛
        // MissingReferenceException，所以先自己清一遍，不指望回调的顺序。
        interactions.RemoveAll(interaction => IsAlive(interaction) == false);

        IInteractable closest = FindClosetInteraction();
        if (closest == current) return;

        current = closest;
        RefreshPrompt(current);
    }

    /// <summary>
    /// 切换头顶提示。旧提示用【缓存的组件引用】关闭，而不是回头去问旧目标——
    /// 旧目标很可能已经被销毁（比如刚被捡走的物品）。
    /// </summary>
    private void RefreshPrompt(IInteractable interaction)
    {
        if (currentPrompt != null)
            currentPrompt.HidePrompt();
        currentPrompt = null;

        if (interaction == null) return;

        currentPrompt = interaction.InteractionTransform.GetComponent<InteractionPrompt>();
        if (currentPrompt == null) return;

        currentPrompt.ShowPrompt(interaction.GetInteractionPrompt(new InteractionContext(interactor)));
    }

    /// <summary>
    /// 这个交互对象还活着吗？
    ///
    /// Unity 给 UnityEngine.Object 重载了 ==，让已销毁的对象与 null 比较时返回 true
    /// （所谓「假 null」）。但运算符重载按【编译期类型】分派，而这里的引用是接口类型，
    /// 编译器只会走 C# 原生的引用比较，保护失效。
    /// 先 as 回 UnityEngine.Object，重载才会重新生效。
    ///
    /// 同理：对 Unity 对象不要用 ?. 和 ??，它们是 C# 语法，同样绕过这个重载。
    /// </summary>
    private static bool IsAlive(IInteractable interaction)
    {
        return interaction as Object != null;
    }

    public void TryInteract()
    {
        IInteractable interaction = current;
        if (IsAlive(interaction) == false)   // 同时挡住 null 和「已销毁但引用还在」
        {
            Debug.Log("Did not find valide interaction");
            return;
        }
        InteractionContext context = new InteractionContext(interactor);
        interaction.Interact(context);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interaction = collision.GetComponent<IInteractable>();
        if (interaction == null) return;
        if (interactions.Contains(interaction)) return;
        InteractionContext context = new InteractionContext(interactor);
        if (!interaction.CanInteract(context)) return;
        interactions.Add(interaction);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interaction = collision.GetComponent<IInteractable>();
        if (interactions.Contains(interaction))
        {
            interactions.Remove(interaction);
        }
    }
    private IInteractable FindClosetInteraction()
    {
        IInteractable closestInteraction = null;
        float minDist = Mathf.Infinity;
        foreach (var interaction in interactions)
        {
            float dist = Vector2.Distance(interaction.InteractionTransform.position, interactor.transform.position);
            if (dist < minDist)
            {
                closestInteraction = interaction;
                minDist = dist;
            }
        }
        return closestInteraction;
    }
}
