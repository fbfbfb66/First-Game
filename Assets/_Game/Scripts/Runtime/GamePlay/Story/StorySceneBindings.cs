using System;
using System.Collections.Generic;
using UnityEngine;

public class StorySceneBindings : MonoBehaviour
{
    [Serializable]
    private class BindingEntry
    {
        public StoryBindingKey key;
        public GameObject target;
    }

    [SerializeField] private List<BindingEntry> bindings = new();

    private readonly Dictionary<StoryBindingKey,GameObject> targetByKey = new();

    private void Awake()
    {
        BuildLookUp();
    }

    public bool TryGetGameObject(StoryBindingKey key,out GameObject target)
    {
        target = null;
        if(key == null) return false;

        return targetByKey.TryGetValue(key,out target);
    }
    public GameObject GetGameObject(StoryBindingKey key)
    {
        if(TryGetGameObject(key,out GameObject target))
        {
            return target;
        }
        string keyName = key != null ? key.name : "Null";
        Debug.LogWarning($"Story binding target not found. Key: {keyName}");

        return null;
    }

    public bool TryGetComponent<T> (StoryBindingKey key,out T component) where T : Component
    {
        component = null;
        if(!TryGetGameObject(key,out GameObject target)) return false;
        if(target == null) return false;
        return target.TryGetComponent(out component);
    }

    public T GetComponent<T> (StoryBindingKey key) where T : Component
    {
        if(TryGetComponent(key,out T component))
        {
            return component;
        }
        string keyName = key != null ? key.name : "Null";
        Debug.LogWarning($"Story binding component not found. Key: {keyName}, Component: {typeof(T).Name}");

        return null;
    }
    private void BuildLookUp()
    {
        targetByKey.Clear();

        foreach(var entry in bindings)
        {
            if(entry == null) continue;

            if(entry.key == null)
            {
                Debug.LogWarning($"Story binding has null key. Index: {entry}");
                continue;
            }
            if(entry.target == null)
            {
                Debug.LogWarning($"Story binding has null target. Key: {entry.key.name}");
                continue;
            }
            if (targetByKey.ContainsKey(entry.key))
            {
                Debug.LogWarning($"Duplicate story binding key. Key: {entry.key.name}");
                continue;
            }
            targetByKey.Add(entry.key,entry.target);
        }
    }
}
