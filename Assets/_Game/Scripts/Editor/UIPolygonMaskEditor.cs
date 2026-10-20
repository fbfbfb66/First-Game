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
        EditorGUILayout.HelpBox("Use Add Closed Region to add a path, then edit it with Polygon Collider 2D > Edit Collider. Paths are combined as filled regions (concave is OK; self-crossing paths and holes are not supported). The disabled collider only stores UI points; leave it disabled. Points use local UI units; resizing the RectTransform does not rescale the outlines.", MessageType.Info);
        UIPolygonMask graphic = (UIPolygonMask)target;
        EditorGUILayout.LabelField("Closed Regions", graphic.Polygon.pathCount.ToString());
        if (GUILayout.Button("Add Closed Region"))
            AddClosedRegion(graphic);
        if (GUILayout.Button("Fit Outline To Rect (Replaces All Regions)"))
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

    private static void AddClosedRegion(UIPolygonMask graphic)
    {
        PolygonCollider2D polygon = graphic.Polygon;
        Undo.RecordObject(polygon, "Add polygon mask region");
        Rect rect = graphic.rectTransform.rect;
        Vector2 center = rect.center - polygon.offset;
        Vector2 half = new Vector2(Mathf.Max(1f, rect.width * 0.1f), Mathf.Max(1f, rect.height * 0.25f));
        int index = polygon.pathCount;
        polygon.pathCount = index + 1;
        polygon.SetPath(index, new[]
        {
            center + new Vector2(-half.x, -half.y), center + new Vector2(half.x, -half.y),
            center + new Vector2(half.x, half.y), center + new Vector2(-half.x, half.y)
        });
        graphic.SetVerticesDirty();
        EditorUtility.SetDirty(polygon);
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
