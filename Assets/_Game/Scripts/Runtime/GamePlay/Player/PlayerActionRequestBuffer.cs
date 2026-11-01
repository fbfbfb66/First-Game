using UnityEngine;

// Each player controller owns a separate buffer. Enemy controllers do not use input buffering.
public sealed class PlayerActionRequestBuffer
{
    private ActionRequest bufferedRequest;
    private bool hasBufferedRequest;

    public bool HasBufferedRequest => hasBufferedRequest;
    public ActionRequest BufferedRequest => bufferedRequest;

    public void RequestAction(ActionCommandKey command, Object owner)
    {
        if (command == null)
        {
            Debug.LogWarning($"[Action] {owner.name}: command is missing; request ignored.", owner);
            return;
        }

        bufferedRequest = new ActionRequest(command, Time.time);
        hasBufferedRequest = true;
    }

    public bool TryGetValidBufferedRequest(ActionSystemConfig config, Object owner, out ActionRequest request)
    {
        request = default;
        if (!hasBufferedRequest) return false;
        if (config == null)
        {
            Debug.LogWarning($"[Action] {owner.name}: system config is missing; request cannot be validated.", owner);
            return false;
        }

        if (Time.time - bufferedRequest.RequestedTime > config.InputBufferDuration)
        {
            Clear();
            return false;
        }

        request = bufferedRequest;
        return true;
    }

    public bool TryConsumeBufferedRequest(ActionCommandKey expectedCommand, ActionSystemConfig config, Object owner)
    {
        if (expectedCommand == null) return false;
        if (!TryGetValidBufferedRequest(config, owner, out var request) || request.Command != expectedCommand)
            return false;

        Clear();
        return true;
    }

    public void Clear()
    {
        bufferedRequest = default;
        hasBufferedRequest = false;
    }
}
