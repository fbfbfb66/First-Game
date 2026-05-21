using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorySequenceRunner : MonoBehaviour
{
    [SerializeField] private GameLayerStack layerStack;
    [SerializeField] private GameFlagCenter flagCenter;
    [SerializeField] private QuestManager questManager;

    private Coroutine currentSequenceCoroutine;

    public bool IsRunning => currentSequenceCoroutine != null;

    private void Awake()
    {
        if(layerStack == null)
            layerStack = FindAnyObjectByType<GameLayerStack>();
        if(flagCenter == null)
            flagCenter = FindAnyObjectByType<GameFlagCenter>();
        if(questManager == null)
            questManager = FindAnyObjectByType<QuestManager>();
    }

    public bool TryRunSequence(StorySequence sequence,GameObject instigator)
    {
        if(sequence == null)
        {
            Debug.LogWarning("Cannot run story sequence. Sequence is null.");
            return false;
        }
        if (IsRunning)
        {
            Debug.LogWarning($"Cannot run story sequence. Another sequence is already running: {sequence.SequenceID}");
            return false;
        }
        currentSequenceCoroutine = StartCoroutine(RunSequenceCoroutine(sequence,instigator));
        return true;
    }

    private IEnumerator RunSequenceCoroutine(StorySequence sequence,GameObject instigator)
    {
        StoryContext context = new StoryContext(this,layerStack,flagCenter,questManager,instigator);

        IReadOnlyList<StoryStepBehaviour> steps = sequence.Steps;

        foreach(var step in steps)
        {
            if(step == null)
            {
                Debug.LogWarning($"Story sequence has a null step. Sequence: {sequence.SequenceID}");
                continue;
            }
            yield return step.Execute(context);
        }
        currentSequenceCoroutine = null;
    }
}
