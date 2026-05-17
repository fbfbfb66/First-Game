using UnityEngine;

public class DebugGameSignalPublisher : MonoBehaviour
{
    [SerializeField] private GameEventBus gameEventBus;
    [SerializeField] private string signalID = "TestSignal";
    [SerializeField] private GameObject instigator;
    private void Awake()
    {
        if(gameEventBus == null)
        {
            gameEventBus = FindAnyObjectByType<GameEventBus>();
        }
    }
    [ContextMenu("Publish Test Signal")]
    private void PublishTestSignal()
    {
        if (gameEventBus == null)
        {
            Debug.LogError("GameEventBus reference is missing on DebugGameSignalPublisher.");
            return;
        }

        var gameSignalEvent = new GameSignalEvent(signalID, gameObject, instigator);
        gameEventBus.Publish(gameSignalEvent);
    }
}
