using TMPro;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;

    public void ShowPrompt(string prompt)
    {
        text.text = prompt;
        text.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        text.gameObject.SetActive(false);
    }
}
