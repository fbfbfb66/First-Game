using UnityEngine;
using System;
[DefaultExecutionOrder(200)]
public class ParallaxController : MonoBehaviour
{
    [SerializeField] private Backgrounds[] backgrounds;
    [SerializeField] private Camera camera;
    private float lastPosx;
    private float currentPosx;

    private void Awake()
    {
        if (camera == null)
        {
            Debug.LogWarning("camera is null");
            return;
        }
        lastPosx = camera.transform.position.x;
    }
    private void LateUpdate()
    {
        currentPosx = camera.transform.position.x;
        foreach (var background in backgrounds)
        {
            background.Move(currentPosx - lastPosx);
        }
        lastPosx = currentPosx;
    }
}
[Serializable]
public sealed class Backgrounds
{
    [SerializeField] private Transform background;
    [SerializeField] private float factor;

    public void Move(float dx)
    {
        if (dx == 0) return;
        Vector2 pos = background.position;
        pos.x += dx*factor;
        background.position = pos;
    }
}
