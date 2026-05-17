using UnityEngine;
using TMPro;
public class WorldDialogueView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text lineText;

    private void Awake()
    {
        Hide();
    }

    public void ShowLine(DialogueLine line)
    {
        if(line == null)
        {
            return;
        }
        SetVisible(true);
        if(lineText != null)
        {
            lineText.text = line.LineText;
        }
    }
    public void Hide()
    {
        SetVisible(false);
        if(lineText != null)
        {
            lineText.text = string.Empty;
        }
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
