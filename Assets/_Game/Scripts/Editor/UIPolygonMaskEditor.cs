using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(UIPolygonMask))]
public sealed class UIPolygonMaskEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Edit the outline with Polygon Collider 2D > Edit Collider. The disabled collider only stores UI points; leave it disabled. One simple closed path is supported (concave is OK, crossing edges are not). Points use local UI units; resizing the RectTransform does not rescale the outline.", MessageType.Info);
        UIPolygonMask graphic = (UIPolygonMask)target;
        if (GUILayout.Button("Fit Outline To Rect"))
        {
            Undo.RecordObject(graphic.Polygon, "Fit polygon mask to rect");
            graphic.FitToRect();
            EditorUtility.SetDirty(graphic.Polygon);
        }
        if (GUILayout.Button("Select Polygon Collider"))
        {
            Selection.activeGameObject = graphic.gameObject;
            EditorGUIUtility.PingObject(graphic.Polygon);
        }
    }

    [MenuItem("GameObject/UI/Polygon Mask", false, 2060)]
    private static void Create(MenuCommand command)
    {
        GameObject parent = command.context as GameObject;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            EditorUtility.DisplayDialog("Polygon Mask", "Select an object under a Canvas first.", "OK");
            return;
        }
        GameObject go = new GameObject("PolygonMask", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create UI polygon mask");
        GameObjectUtility.SetParentAndAlign(go, parent);
        RectTransform rect = (RectTransform)go.transform;
        rect.sizeDelta = new Vector2(600, 60);
        UIPolygonMask graphic = Undo.AddComponent<UIPolygonMask>(go);
        graphic.FitToRect();
        graphic.raycastTarget = false;
        go.GetComponent<Mask>().showMaskGraphic = false;
        Selection.activeGameObject = go;
    }
}
