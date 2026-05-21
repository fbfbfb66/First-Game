using Unity.InferenceEngine.Tokenization.PostProcessors.Templating;
using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    [SerializeField] private StorySequenceRunner sequenceRunner;
    [SerializeField] private StorySequence storySequence;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void Awake()
    {
        if(sequenceRunner == null)
            sequenceRunner = FindAnyObjectByType<StorySequenceRunner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(triggerOnce && hasTriggered) return;

        if(!collision.GetComponent<Player>()) return;

        if(sequenceRunner == null) return;

        if(storySequence == null) return;

        bool started = sequenceRunner.TryRunSequence(storySequence,collision.gameObject);
        if (started)
        {
            hasTriggered = true;
        }
    }
}
