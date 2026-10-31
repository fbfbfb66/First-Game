using UnityEngine;

public class WorldTimeAnimator : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;

    private float originalSpeed;

    private void Awake()
    {
        if (targetAnimator == null)
            targetAnimator = GetComponent<Animator>();

        if (targetAnimator == null)
        {
            Debug.LogWarning("WorldTimeAnimator: Animator is missing.", this);
            enabled = false;
            return;
        }

        originalSpeed = targetAnimator.speed;
    }

    private void Update()
    {
        if (targetAnimator != null)
            targetAnimator.speed = originalSpeed * WorldTime.TimeScale;
    }

    private void OnDisable()
    {
        if (targetAnimator != null)
            targetAnimator.speed = originalSpeed;
    }
}
