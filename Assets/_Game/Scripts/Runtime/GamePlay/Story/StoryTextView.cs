using TMPro;
using UnityEngine;

public class StoryTextView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI contentText;

    private void Awake()
    {
        Hide();
    }
    public void ShowText(string content)
    {
        SetVisible(true);
        if(contentText != null)
        {
            contentText.text = content;
        }
    }
    public void Hide()
    {
        if(contentText != null)
        {
            contentText.text = string.Empty;
        }
        SetVisible(false);
    }
    private void SetVisible(bool visible)
    {
        if(root != null)
        {
            root.SetActive(visible);
            return;
        }

        gameObject.SetActive(visible);
    }
}
