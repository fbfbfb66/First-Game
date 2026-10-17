using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Reuses BootScene services when starting Play from GameScene, then skips the menu.
/// </summary>
[InitializeOnLoad]
public static class DirectGameScenePlay
{
    private const string MenuPath = "Tools/Game/Direct Play From GameScene";
    private const string RequestKey = "FirstGame.DirectPlay.Request";
    private const string OverrideKey = "FirstGame.DirectPlay.Override";
    private const string PreviousSceneKey = "FirstGame.DirectPlay.PreviousScene";
    private const string BootPath = "Assets/_Game/Scenes/BootScene.unity";
    private const string GamePath = "Assets/_Game/Scenes/GameScene.unity";
    private static string EnabledKey => Application.dataPath + ".DirectGameScenePlay.Enabled";

    static DirectGameScenePlay()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        EditorPrefs.SetBool(EnabledKey, !EditorPrefs.GetBool(EnabledKey, true));
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateToggle()
    {
        Menu.SetChecked(MenuPath, EditorPrefs.GetBool(EnabledKey, true));
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            SessionState.SetBool(RequestKey, false);
            if (!EditorPrefs.GetBool(EnabledKey, true) || SceneManager.GetActiveScene().path != GamePath)
                return;

            SceneAsset boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootPath);
            if (boot == null)
            {
                Debug.LogError("Direct GameScene Play failed: BootScene asset is missing.");
                return;
            }

            SessionState.SetString(PreviousSceneKey, AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            SessionState.SetBool(OverrideKey, true);
            SessionState.SetBool(RequestKey, true);
            EditorSceneManager.playModeStartScene = boot;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (SessionState.GetBool(OverrideKey, false))
            {
                string previousPath = SessionState.GetString(PreviousSceneKey, "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previousPath)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<SceneAsset>(previousPath);
            }
            SessionState.SetBool(RequestKey, false);
            SessionState.SetBool(OverrideKey, false);
        }
    }
}
