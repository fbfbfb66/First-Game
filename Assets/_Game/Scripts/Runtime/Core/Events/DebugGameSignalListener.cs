using UnityEngine;

public class DebugGameSignalListener : MonoBehaviour
{
    [SerializeField] private GameEventBus gameEventBus;
    private void Awake()
    {
        if(gameEventBus == null)
        {
            gameEventBus = FindAnyObjectByType<GameEventBus>();
        }
    }
    private void OnEnable()
    {
        if(gameEventBus == null)
        {
            Debug.LogError("GameEventBus reference is missing on DebugGameSignalListener.");
            return;
        }
        gameEventBus.Subscribe<GameSignalEvent>(OnGameSignal);
    }
    private void OnDisable()
    {
        if(gameEventBus == null)
        {
            return;
        }
        gameEventBus.Unsubscribe<GameSignalEvent>(OnGameSignal);
    }
    private void OnGameSignal(GameSignalEvent gameSignalEvent)
    {
        string senderName = gameSignalEvent.Sender != null
            ? gameSignalEvent.Sender.name
            : "None";

        string instigatorName = gameSignalEvent.Instigator != null
            ? gameSignalEvent.Instigator.name
            : "None";

        Debug.Log(
            $"Received GameSignalEvent. " +
            $"SignalId: {gameSignalEvent.SignalID}, " +
            $"Sender: {senderName}, " +
            $"Instigator: {instigatorName}"
        );
    }
}
