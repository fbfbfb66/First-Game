using Unity.Cinemachine;
using UnityEngine;

public class StoryCameraDirector : MonoBehaviour
{
    [SerializeField] private int activePriority = 100;
    [SerializeField] private int inactivePriority = 0;
    [SerializeField] private bool lowerPreviousCameera = true;

    private CinemachineVirtualCameraBase currentCamera;
    public CinemachineVirtualCameraBase CurrentCamera => currentCamera;

    public bool SwitchTo(StorySceneBindings sceneBindings,StoryBindingKey targetCameraKey)
    {
        if(sceneBindings == null)
        {
            Debug.LogWarning("SceneBindings is null, cannot switch camera.");
            return false;
        }
        CinemachineVirtualCameraBase targetCamera = sceneBindings.GetComponent<CinemachineVirtualCameraBase>(targetCameraKey);
        if(targetCamera == null) return false;

        if(lowerPreviousCameera && currentCamera != null && currentCamera != targetCamera)
        {
            currentCamera.Priority = inactivePriority;
        }
        currentCamera = targetCamera;
        currentCamera.Priority = activePriority;
        return true;
    }
}
