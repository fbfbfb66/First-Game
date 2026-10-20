using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Keeps the HP/MP fill transforms read-only in the Inspector.</summary>
[InitializeOnLoad]
internal static class HudFillInspectorLock
{
    private static readonly string[] FillPaths =
    {
        "HUD/HP/MaskArea/Fill",
        "HUD/MP/MaskArea/Fill"
    };

    static HudFillInspectorLock()
    {
        EditorApplication.hierarchyChanged += ScheduleApply;
        ScheduleApply();
    }

    private static void ScheduleApply()
    {
        EditorApplication.delayCall -= ApplyToLoadedScenes;
        EditorApplication.delayCall += ApplyToLoadedScenes;
    }

    private static void ApplyToLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != "Canvas_HUD") continue;

                foreach (string path in FillPaths)
                {
                    var fill = root.transform.Find(path) as RectTransform;
                    if (fill == null || (fill.hideFlags & HideFlags.NotEditable) != 0) continue;
                    // Editor-only state: do not dirty/save the user's scene.
                    fill.hideFlags |= HideFlags.NotEditable;
                }
            }
        }

        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
