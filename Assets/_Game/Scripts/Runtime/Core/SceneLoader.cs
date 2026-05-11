using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.Log("SceneName 没有命名");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void LoadMainMenu()
    {
        LoadScene(SceneNames.MainMenuScene);
    }
    public void LoadGameScene()
    {
        LoadScene(SceneNames.GameScene);
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}
