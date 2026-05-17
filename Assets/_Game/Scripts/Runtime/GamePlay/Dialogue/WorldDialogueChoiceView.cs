using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldDialogueChoiceView : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private List<TMP_Text> choiceSlots;

    [Header("Input")]
    [FormerlySerializedAs("nvaigationCooldown")]
    [SerializeField] private float navigationCooldown = 0.1f;

    private IReadOnlyList<DialogueChoice> currentChoices;
    private int selectedIndex;
    private float nextNavigationTime;

    private void Awake()
    {
        Hide();
    }
    public DialogueChoice GetSelectedChoice()
    {
        if(currentChoices == null || currentChoices.Count == 0) return null;
        return currentChoices[selectedIndex];
    }

    public void ShowChoices(IReadOnlyList<DialogueChoice> choices)
    {
        if(choices == null || choices.Count == 0)
        {
            Hide();
            return;
        }
        currentChoices = choices;
        selectedIndex = 0;
        nextNavigationTime = 0f;
        RefreshAllChoiceSlots();
        SetVisible(true);
    }

    public void MoveSelection(Vector2 navigateInput)
    {
        if(currentChoices == null || currentChoices.Count == 0)
        {
            return;
        }
        if(Time.time < nextNavigationTime)
        {
            return;
        }

        if(Mathf.Abs(navigateInput.y) < 0.5f)
        {
            return;
        }

        int previousSelectedIndex = selectedIndex;

        if(navigateInput.y > 0)
        {
            selectedIndex--;
        }
        else if(navigateInput.y < 0)
        {
            selectedIndex++;
        }
        
        selectedIndex = ((selectedIndex % currentChoices.Count) + currentChoices.Count) % currentChoices.Count;

        nextNavigationTime = Time.time + navigationCooldown;
        if(selectedIndex != previousSelectedIndex)
        {
            RefreshChoiceSlot(previousSelectedIndex);
            RefreshChoiceSlot(selectedIndex);
        }
    }
    public void Hide()
    {
        currentChoices = null;
        selectedIndex = 0;
        nextNavigationTime = 0f;
        SetVisible(false);
    }
    private void RefreshAllChoiceSlots()
    {
        for(int i = 0; i < choiceSlots.Count; i++)
        {
            RefreshChoiceSlot(i);
        }
    }
    private void RefreshChoiceSlot(int index)
    {
        if(index < 0 || index >= choiceSlots.Count)
        {
            return;
        }
        TMP_Text slot = choiceSlots[index];
        if(slot == null)
        {
            return;
        }
        if(index >= currentChoices.Count)
        {
            slot.gameObject.SetActive(false);
            slot.text = string.Empty;
            return;
        }
        slot.gameObject.SetActive(true);

        string prefix = index == selectedIndex ? "> " : string.Empty;
        slot.text = prefix + currentChoices[index].ChoiceText;
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
