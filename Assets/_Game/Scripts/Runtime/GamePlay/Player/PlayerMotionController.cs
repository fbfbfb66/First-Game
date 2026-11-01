using UnityEngine;

public class PlayerMotionController : MotionController
{
    [SerializeField] private ActionSystemConfig config;
    private readonly PlayerActionRequestBuffer requestBuffer = new();

    public bool HasBufferedRequest => requestBuffer.HasBufferedRequest;
    public ActionRequest BufferedRequest => requestBuffer.BufferedRequest;
    public float RequestBufferDuration => config != null ? config.InputBufferDuration : 0f;

    public void RequestAction(ActionCommandKey command) => requestBuffer.RequestAction(command, this);

    public bool TryGetValidBufferedRequest(out ActionRequest request)
        => requestBuffer.TryGetValidBufferedRequest(config, this, out request);

    public bool TryConsumeBufferedRequest(ActionCommandKey expectedCommand)
        => requestBuffer.TryConsumeBufferedRequest(expectedCommand, config, this);

    public override bool TryStartAction(MotionActionDefinition action, in ActionContext context)
    {
        if (action != null && action.NeedCommand)
        {
            if (!TryGetValidBufferedRequest(out var request) || action.Command == null || request.Command != action.Command)
                return false;
        }

        if (!base.TryStartAction(action, in context)) return false;
        if (action.NeedCommand) requestBuffer.Clear();
        return true;
    }
}
