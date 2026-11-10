using UnityEngine;

public class ActiveArea : MonoBehaviour
{
    [SerializeField] private float width = 10f;
    [SerializeField] private float height = 10f;
    [SerializeField, Min(0f)] private float targetBuffer = 1f;

    public float TargetBuffer => targetBuffer;

    public float DiagonalLength => new Vector2(width, height).magnitude;

    public float LeftXPosition => transform.position.x - width / 2f;
    public float RightXPosition => transform.position.x + width / 2f;

    public float BottomYPosition => transform.position.y - height / 2f;
    public float TopYPosition => transform.position.y + height / 2f;

    public bool Contains(Vector2 position) =>
        position.x >= LeftXPosition && position.x <= RightXPosition &&
        position.y >= BottomYPosition && position.y <= TopYPosition;

    public bool ContainsTarget(Vector2 position)
    {
        float buffer = Mathf.Max(0f, targetBuffer);
        return position.x >= LeftXPosition - buffer && position.x <= RightXPosition + buffer &&
            position.y >= BottomYPosition - buffer && position.y <= TopYPosition + buffer;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0f));
        Color previousColor = Gizmos.color;
        Gizmos.color = Color.yellow;
        float buffer = Mathf.Max(0f, targetBuffer);
        Gizmos.DrawWireCube(transform.position, new Vector3(width + buffer * 2f, height + buffer * 2f, 0f));
        Gizmos.color = previousColor;
    }
}
