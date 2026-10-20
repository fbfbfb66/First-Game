using UnityEngine;

[DefaultExecutionOrder(300)]
[DisallowMultipleComponent]
public sealed class BackgroundLoopController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [Tooltip("Drag the SpriteRenderer of the middle image under this object here.")]
    [SerializeField] private SpriteRenderer middleImage;
    [Tooltip("World units removed from each side of the trigger bounds. Does not change the teleport distance.")]
    [SerializeField, Min(0f)] private float offset;

    private bool hasWarned;

    private void LateUpdate()
    {
        if (targetCamera == null || middleImage == null || middleImage.sprite == null)
        {
            WarnOnce("Assign Target Camera and a Middle Image with a sprite; looping was skipped.");
            return;
        }

        if (!targetCamera.orthographic || !middleImage.transform.IsChildOf(transform))
        {
            WarnOnce("Use an orthographic camera and a Middle Image under this object; looping was skipped.");
            return;
        }

        Bounds bounds = middleImage.bounds;
        float width = bounds.size.x;
        float inset = Mathf.Max(0f, offset);
        float cameraHalfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        if (!IsLoopRangeValid(width, inset, cameraHalfWidth))
        {
            WarnOnce("Twice Offset must be smaller than both the image width and the camera view width; looping was skipped to prevent crossed boundaries or alternating teleports.");
            return;
        }

        hasWarned = false;
        float cameraX = targetCamera.transform.position.x;
        float cameraLeft = cameraX - cameraHalfWidth;
        float cameraRight = cameraX + cameraHalfWidth;
        float leftBoundary = bounds.min.x + inset;
        float rightBoundary = bounds.max.x - inset;
        float shift = 0f;

        if (cameraLeft >= rightBoundary)
            shift = Mathf.Floor((cameraLeft - rightBoundary) / width + 1f) * width;
        else if (cameraRight <= leftBoundary)
            shift = -Mathf.Floor((leftBoundary - cameraRight) / width + 1f) * width;

        if (shift != 0f)
            transform.position += Vector3.right * shift;
    }

    private void OnDrawGizmosSelected()
    {
        if (middleImage == null || middleImage.sprite == null) return;

        Bounds bounds = middleImage.bounds;
        float inset = Mathf.Max(0f, offset);
        float left = bounds.min.x + inset;
        float right = bounds.max.x - inset;
        float z = bounds.center.z;
        Color previousColor = Gizmos.color;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, new Vector3(bounds.size.x, bounds.size.y, 0f));

        bool valid = targetCamera != null && targetCamera.orthographic
            && middleImage.transform.IsChildOf(transform)
            && IsLoopRangeValid(bounds.size.x, inset, targetCamera.orthographicSize * targetCamera.aspect);
        Gizmos.color = valid ? Color.green : Color.red;
        Gizmos.DrawLine(new Vector3(left, bounds.min.y, z), new Vector3(left, bounds.max.y, z));
        Gizmos.DrawLine(new Vector3(right, bounds.min.y, z), new Vector3(right, bounds.max.y, z));
        Gizmos.DrawLine(new Vector3(bounds.min.x, bounds.center.y, z), new Vector3(left, bounds.center.y, z));
        Gizmos.DrawLine(new Vector3(bounds.max.x, bounds.center.y, z), new Vector3(right, bounds.center.y, z));

#if UNITY_EDITOR
        UnityEditor.Handles.Label(new Vector3(left, bounds.max.y, z), $"Left trigger | Offset {inset:0.###}");
        UnityEditor.Handles.Label(new Vector3(right, bounds.max.y, z), $"Right trigger | Offset {inset:0.###}");
#endif
        Gizmos.color = previousColor;
    }

    private static bool IsLoopRangeValid(float imageWidth, float inset, float cameraHalfWidth)
    {
        // After shifting by one image width, the opposite trigger has
        // cameraViewWidth - 2 * inset clearance. It must remain positive.
        return imageWidth > inset * 2f && cameraHalfWidth > inset;
    }

    private void WarnOnce(string reason)
    {
        if (hasWarned) return;
        Debug.LogWarning($"{nameof(BackgroundLoopController)} on '{name}': {reason}", this);
        hasWarned = true;
    }
}
