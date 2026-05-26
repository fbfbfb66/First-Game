using System.Collections.Generic;
using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    [SerializeField] private StorySequenceRunner sequenceRunner;
    [SerializeField] private StorySequence storySequence;
    [SerializeField] private bool triggerOnce = true;
    [Header("Start Conditions")]
    [SerializeField] private List<GameCondition> startConditions = new();
    [SerializeField] private bool requireAllConditions = true;
    [Header("Runtime Services")]
    [SerializeField] private GameFlagCenter flagCenter;
    [SerializeField] private QuestManager questManager;

    private bool hasTriggered;

    private void Awake()
    {
        if(sequenceRunner == null)
            sequenceRunner = FindAnyObjectByType<StorySequenceRunner>();
        if(flagCenter == null)
            flagCenter = FindAnyObjectByType<GameFlagCenter>();
        if(questManager == null)
            questManager = FindAnyObjectByType<QuestManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(triggerOnce && hasTriggered) return;

        if(!collision.GetComponent<Player>()) return;

        GameObject instigator = collision.gameObject;

        if (!CanStart(instigator))
        {
            return;
        }

        if(sequenceRunner == null) return;

        if(storySequence == null) return;

        bool started = sequenceRunner.TryRunSequence(storySequence,collision.gameObject);
        if (started)
        {
            hasTriggered = true;
        }
    }
    private bool CanStart(GameObject instigator)
    {
        if (startConditions == null || startConditions.Count == 0)
        {
            return true;
        }

        GameConditionContext conditionContext = new GameConditionContext(
            flagCenter,
            questManager,
            gameObject,
            instigator
        );

        if (requireAllConditions)
        {
            return AreAllConditionsMet(conditionContext);
        }

        return IsAnyConditionMet(conditionContext);
    }
    private bool AreAllConditionsMet(GameConditionContext context)
    {
        for (int i = 0; i < startConditions.Count; i++)
        {
            GameCondition condition = startConditions[i];

            if (condition == null)
            {
                return false;
            }

            if (!condition.IsMet(context))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsAnyConditionMet(GameConditionContext context)
    {
        for (int i = 0; i < startConditions.Count; i++)
        {
            GameCondition condition = startConditions[i];

            if (condition == null)
            {
                continue;
            }

            if (condition.IsMet(context))
            {
                return true;
            }
        }

        return false;
    }
}
