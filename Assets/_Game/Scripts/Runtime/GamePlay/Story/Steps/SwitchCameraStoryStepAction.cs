using System.Collections;
using UnityEngine;
[CreateAssetMenu(
    fileName = "SwitchCameraStoryStep",
    menuName = "Game/Story/Step/Switch Camera")]
public class SwitchCameraStoryStepAction : StoryStepAction
{
    [SerializeField] private StoryBindingKey cameraDirectorKey;
    [SerializeField] private StoryBindingKey targetCameraKey;

    [Min(0f)]
    [SerializeField] private float waitAfterSwitch = 1f;

    public override IEnumerator Execute(StoryContext context)
    {
        if(context.SceneBindings == null)
        {
            Debug.LogWarning("SceneBindings is null, cannot switch camera.");
            yield break;
        }

        StoryCameraDirector cameraDirector = context.SceneBindings.GetComponent<StoryCameraDirector>(cameraDirectorKey);
        if(cameraDirector == null) yield break;

        bool switched = cameraDirector.SwitchTo(context.SceneBindings, targetCameraKey);
        if(!switched) yield break;

        if(waitAfterSwitch > 0f)
        {
            yield return new WaitForSeconds(waitAfterSwitch);
        }
    }
}
