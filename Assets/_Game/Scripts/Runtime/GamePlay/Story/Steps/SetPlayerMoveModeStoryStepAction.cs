using System.Collections;
using UnityEngine;
[CreateAssetMenu(
    fileName = "SetPlayerMoveModeStoryStep",
    menuName = "Game/Story/Step/Set Player Move Mode")]
public class SetPlayerMoveModeStoryStepAction : StoryStepAction
{
    [SerializeField] private StoryBindingKey playerKey;
    [SerializeField] private PlayerMoveType moveType = PlayerMoveType.Walk;

    public override IEnumerator Execute(StoryContext context)
    {
        if (context.SceneBindings == null)
        {
            Debug.LogWarning("Cannot set player move mode. StorySceneBindings is missing.");
            yield break;
        }
        PlayerMovement playerMovement = context.SceneBindings.GetComponent<PlayerMovement>(playerKey);
        if(playerMovement == null) yield break;
        playerMovement.SetPlayerMoveMode(moveType);
        yield break;
    }
}
