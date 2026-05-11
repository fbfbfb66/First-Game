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
        sceneLoader.LoadMainMenu();
        layerStack.ResetTo(GameLayerType.MainMenu);
    }
}
