using UnityEngine;

[RequireComponent(typeof(SceneLoader))]
[RequireComponent(typeof(GameLayerStack))]
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private GameLayerStack layerStack;

    private void Awake()
    {
        if(sceneLoader == null)
            sceneLoader = GetComponent<SceneLoader>();

        if(layerStack == null)
            layerStack = GetComponent<GameLayerStack>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (UnityEditor.SessionState.GetBool("FirstGame.DirectPlay.Request", false))
        {
            UnityEditor.SessionState.SetBool("FirstGame.DirectPlay.Request", false);
            sceneLoader.LoadGameScene();
            layerStack.ResetTo(GameLayerType.Gameplay);
            return;
        }
#endif
        sceneLoader.LoadMainMenu();
        layerStack.ResetTo(GameLayerType.MainMenu);
    }
}
