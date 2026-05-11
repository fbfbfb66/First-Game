using UnityEngine.UI;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button quitGameButton;

    private SceneLoader sceneLoader;

    private void Awake()
    {
        sceneLoader = FindAnyObjectByType<SceneLoader>();
    }

    private void OnEnable()
    {
        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueGameButton.onClick.AddListener(OnContinueGameClicked);
        quitGameButton.onClick.AddListener(OnQuitGameClicked);
    }

    private void OnDisable()
    {
        newGameButton.onClick.RemoveListener(OnNewGameClicked);
        continueGameButton.onClick.RemoveListener(OnContinueGameClicked);
        quitGameButton.onClick.RemoveListener(OnQuitGameClicked);
    }

    private void OnNewGameClicked()
    {
        sceneLoader.LoadGameScene();
    }
    private void OnContinueGameClicked()
    {
        Debug.Log("存档系统待开发");
    }
    private void OnQuitGameClicked()
    {
        sceneLoader.QuitGame();
    }
}
