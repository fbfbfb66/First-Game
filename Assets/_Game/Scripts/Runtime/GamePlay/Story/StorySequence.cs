using System.Collections.Generic;
using UnityEngine;

public class StorySequence : MonoBehaviour
{
   [SerializeField] private string sequenceID;
   [SerializeField] private List<StoryStepAction> steps = new();

    public string SequenceID => sequenceID;
    public IReadOnlyList<StoryStepAction> Steps => steps;
}
