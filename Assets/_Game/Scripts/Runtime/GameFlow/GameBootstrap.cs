using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;

    private void Awake()
    {
        sceneLoader = GetComponent<SceneLoader>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        sceneLoader.LoadMainMenue();
    }
}
