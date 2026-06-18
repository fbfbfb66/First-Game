using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(
    fileName = "StopPlayerStoryStep",
    menuName = "Game/Story/Step/Stop Player")]
public class StopPlayerStoryStepAction : StoryStepAction
{
    [SerializeField] private StoryBindingKey playerkey;
    public override IEnumerator Execute(StoryContext context)
    {
        if(context.SceneBindings == null)
        {
            Debug.LogError("Scene Bindings is null in context");
            yield break;
        }
        GameObject playerObject = context.SceneBindings.GetGameObject(playerkey);

        if(playerObject == null)
        {
            yield break;
        }
        ClearPlayerInput(playerObject);
        StopPlayerVelocity(playerObject);
        yield break;
    }

    private void ClearPlayerInput(GameObject player)
    {
        if(!player.TryGetComponent(out PlayerInputReceiver inputReceiver))
        {
            Debug.LogWarning("Player object does not have PlayerInputReceiver component.");
            return;
        }
        inputReceiver.ClearMoveInput();
    }
    private void StopPlayerVelocity(GameObject player)
    {
        if(!player.TryGetComponent(out PlayerMovement movement))
        {
            Debug.LogWarning("Player object does not have PlayerMovement component.");
            return;
        }
        movement.SetRigibodyVelocity(Vector2.zero);
    }
}
