using System.Collections.Generic;
using UnityEngine;

public class StorySequence : MonoBehaviour
{
   [SerializeField] private string sequenceID;
   [SerializeField] private List<StoryStepBehaviour> steps = new();

    public string SequenceID => sequenceID;
    public IReadOnlyList<StoryStepBehaviour> Steps => steps;
}
