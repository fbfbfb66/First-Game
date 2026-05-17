using System;
using System.Collections.Generic;
using UnityEngine;

public class GameFlagCenter : MonoBehaviour
{
    [SerializeField] private GameEventBus eventBus;
    [SerializeField] private GameFlagDatabase initialBoolFlags;

    private readonly Dictionary<string,bool> boolFlags = new();

    private void Awake()
    {
        if(eventBus == null)
        {
            eventBus = FindAnyObjectByType<GameEventBus>();
        }
        InitializeBoolFlags();
    }

    public void SetBool(string flagID,bool value,GameObject sender,GameObject instigator)
    {
        if (string.IsNullOrWhiteSpace(flagID))
        {
            return;
        }
        bool hadPreviousValue = boolFlags.TryGetValue(flagID,out bool previousValue);
        if(hadPreviousValue && previousValue == value)
        {
            return;
        }
        boolFlags[flagID] = value;
        Debug.Log($"Set {flagID} to {value}");
        if(eventBus != null)
        {
            eventBus.Publish(new GameFlagChangedEvent(flagID,hadPreviousValue,previousValue,value,sender,instigator));
        }
    }
    public bool GetBool(string flagID,bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(flagID))
        {
            return defaultValue;
        }

        if(boolFlags.TryGetValue(flagID,out bool value))
        {
            return value;
        }
        return defaultValue;
    }

    public bool HasBool(string flagID)
    {
        if (string.IsNullOrWhiteSpace(flagID))
        {
            return false;
        }
        return boolFlags.ContainsKey(flagID);
    }

    private void InitializeBoolFlags()
    {
        boolFlags.Clear();

        if(initialBoolFlags == null)
        {
            Debug.Log("GameFlagCenter has no initial flag database. Runtime flags will start empty.");
            return;
        }

        for(int i = 0; i < initialBoolFlags.BoolFlags.Count; i++)
        {
            GameFlagEntry entry = initialBoolFlags.BoolFlags[i];

            if(entry == null) continue;
            if(string.IsNullOrWhiteSpace(entry.FlagID)) continue;

            boolFlags[entry.FlagID] = entry.DefaultValue;
        }
    }
}
