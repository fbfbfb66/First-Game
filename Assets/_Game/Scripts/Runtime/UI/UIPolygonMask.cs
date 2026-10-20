using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>A UI stencil mask whose outline is edited with PolygonCollider2D's native tool.</summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(PolygonCollider2D), typeof(Mask))]
[RequireComponent(typeof(CanvasRenderer))]
[AddComponentMenu("UI/Polygon Mask")]
public sealed class UIPolygonMask : MaskableGraphic, ICanvasRaycastFilter
{
    private PolygonCollider2D polygon;
    private uint shapeHash;
    private readonly List<Vector2> points = new List<Vector2>();
    private readonly List<int> remaining = new List<int>();
    private readonly List<int> triangles = new List<int>();
    private readonly List<Vector2> path = new List<Vector2>();
    private readonly List<Vector2> meshPoints = new List<Vector2>();
    private readonly List<int> meshTriangles = new List<int>();
    private string lastWarning;

    public PolygonCollider2D Polygon => polygon != null ? polygon : polygon = GetComponent<PolygonCollider2D>();

    protected override void OnEnable()
    {
        base.OnEnable();
        Polygon.enabled = false;
        shapeHash = Polygon.GetShapeHash();
        SetVerticesDirty();
    }

    protected override void Reset()
    {
        base.Reset();
        raycastTarget = false;
        FitToRect();
        GetComponent<Mask>().showMaskGraphic = false;
    }

    private void Update()
    {
        // The collider is an authoring data source, never a physics obstacle.
        if (Polygon.enabled) Polygon.enabled = false;
        uint current = Polygon.GetShapeHash();
        if (current == shapeHash) return;
        shapeHash = current;
        SetVerticesDirty();
    }

    /// <summary>Replace the outline with the current RectTransform rectangle.</summary>
    public void FitToRect()
    {
        Rect rect = rectTransform.rect;
        Polygon.offset = Vector2.zero;
        Polygon.pathCount = 1;
        Polygon.SetPath(0, new[]
        {
            new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin),
            new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax)
        });
        Polygon.enabled = false;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!BuildPolygons()) return;
        Rect rect = rectTransform.rect;
        foreach (Vector2 point in meshPoints)
        {
            Vector2 uv = new Vector2(
                rect.width == 0 ? 0 : (point.x - rect.xMin) / rect.width,
                rect.height == 0 ? 0 : (point.y - rect.yMin) / rect.height);
            vh.AddVert(point, color, uv);
        }
        for (int i = 0; i < meshTriangles.Count; i += 3)
            vh.AddTriangle(meshTriangles[i], meshTriangles[i + 1], meshTriangles[i + 2]);
    }

    private bool BuildPolygons()
    {
        meshPoints.Clear();
        meshTriangles.Clear();
        for (int pathIndex = 0; pathIndex < Polygon.pathCount; pathIndex++)
        {
            if (!BuildPath(pathIndex))
            {
                meshPoints.Clear();
                meshTriangles.Clear();
                return false;
            }

            int vertexOffset = meshPoints.Count;
            meshPoints.AddRange(points);
            foreach (int index in triangles) meshTriangles.Add(vertexOffset + index);
        }
        lastWarning = null;
        return meshTriangles.Count > 0;
    }

    private bool BuildPath(int pathIndex)
    {
        points.Clear();
        triangles.Clear();
        remaining.Clear();
        Polygon.GetPath(pathIndex, path);
        foreach (Vector2 point in path)
        {
            Vector2 value = point + Polygon.offset;
            if (!float.IsFinite(value.x) || !float.IsFinite(value.y)) return Warn(pathIndex, "Outline contains a non-finite point.");
            if (points.Count == 0 || (points[points.Count - 1] - value).sqrMagnitude > 0.000001f)
                points.Add(value);
        }
        if (points.Count > 1 && (points[0] - points[points.Count - 1]).sqrMagnitude <= 0.000001f)
            points.RemoveAt(points.Count - 1);
        // Remove redundant straight-line vertices before ear clipping.
        bool removed = true;
        while (removed && points.Count > 3)
        {
            removed = false;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 a = points[(i + points.Count - 1) % points.Count];
                Vector2 b = points[i];
                Vector2 c = points[(i + 1) % points.Count];
                if (Mathf.Abs(Cross(b - a, c - b)) > 0.00001f || Vector2.Dot(b - a, c - b) < 0) continue;
                points.RemoveAt(i);
                removed = true;
                break;
            }
        }
        if (points.Count < 3) return Warn(pathIndex, "Outline needs at least three distinct points.");
        for (int i = 0; i < points.Count; i++)
        {
            int nextI = (i + 1) % points.Count;
            for (int j = i + 1; j < points.Count; j++)
            {
                int nextJ = (j + 1) % points.Count;
                if (nextI == j || nextJ == i) continue;
                if (SegmentsIntersect(points[i], points[nextI], points[j], points[nextJ]))
                    return Warn(pathIndex, "Outline crosses or touches itself. Move the points so edges do not intersect.");
            }
        }
        float area = 0;
        for (int i = 0; i < points.Count; i++) area += Cross(points[i], points[(i + 1) % points.Count]);
        if (Mathf.Abs(area) < 0.00001f) return Warn(pathIndex, "Outline has no usable area.");
        for (int i = 0; i < points.Count; i++) remaining.Add(area > 0 ? i : points.Count - 1 - i);
        while (remaining.Count > 3)
        {
            bool found = false;
            for (int i = 0; i < remaining.Count; i++)
            {
                int a = remaining[(i + remaining.Count - 1) % remaining.Count];
                int b = remaining[i];
                int c = remaining[(i + 1) % remaining.Count];
                if (Cross(points[b] - points[a], points[c] - points[b]) <= 0.00001f) continue;
                bool occupied = false;
                foreach (int other in remaining)
                {
                    if (other == a || other == b || other == c) continue;
                    if (!InsideTriangle(points[other], points[a], points[b], points[c])) continue;
                    occupied = true;
                    break;
                }
                if (occupied) continue;
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                remaining.RemoveAt(i);
                found = true;
                break;
            }
            if (!found) return Warn(pathIndex, "Outline could not be triangulated. Check for overlapping or extremely close points.");
        }
        triangles.AddRange(remaining);
        return true;
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!isActiveAndEnabled) return true;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 local)) return false;
        for (int i = 0; i < meshTriangles.Count; i += 3)
            if (InsideTriangle(local, meshPoints[meshTriangles[i]], meshPoints[meshTriangles[i + 1]], meshPoints[meshTriangles[i + 2]])) return true;
        return false;
    }

    private bool Warn(int pathIndex, string reason)
    {
        triangles.Clear();
        reason = $"Path {pathIndex}: {reason}";
        if (lastWarning != reason)
            Debug.LogWarning($"UIPolygonMask '{name}': {reason} Mask geometry is empty until corrected.", this);
        lastWarning = reason;
        return false;
    }

    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    private static bool InsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c) =>
        Cross(b - a, p - a) >= -0.00001f && Cross(c - b, p - b) >= -0.00001f && Cross(a - c, p - c) >= -0.00001f;

    private static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        if (Mathf.Max(a.x, b.x) < Mathf.Min(c.x, d.x) || Mathf.Max(c.x, d.x) < Mathf.Min(a.x, b.x) ||
            Mathf.Max(a.y, b.y) < Mathf.Min(c.y, d.y) || Mathf.Max(c.y, d.y) < Mathf.Min(a.y, b.y)) return false;
        float abC = Cross(b - a, c - a), abD = Cross(b - a, d - a);
        float cdA = Cross(d - c, a - c), cdB = Cross(d - c, b - c);
        return ((abC >= 0 && abD <= 0) || (abC <= 0 && abD >= 0)) &&
               ((cdA >= 0 && cdB <= 0) || (cdA <= 0 && cdB >= 0));
    }
}
