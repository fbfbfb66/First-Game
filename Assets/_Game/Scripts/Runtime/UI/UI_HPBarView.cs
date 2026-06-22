using UnityEngine;

public class UI_HPBarView : MonoBehaviour
{
    [SerializeField] private RectTransform fillClip;
    private float fullWidth = 600f;

    public void UpdateHpBar(float value)
    {
        if(fillClip == null) return;
        if(value < 0f || value > 1f)
        {
            Debug.LogError("Value must be between 0 and 1");
            return;
        } 
            
        fillClip.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullWidth * value);
    }
}
