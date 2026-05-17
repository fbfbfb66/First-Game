using UnityEngine;

public abstract class GameCondition : ScriptableObject
{
    public abstract bool IsMet(GameConditionContext context);
}
