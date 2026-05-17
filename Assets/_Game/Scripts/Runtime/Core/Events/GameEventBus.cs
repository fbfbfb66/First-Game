using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEventBus : MonoBehaviour
{
    private readonly Dictionary<Type,Delegate> eventTable = new();

    public void Subscribe<T>(Action<T> listener) where T : IGameEvent
    {
        Type type = typeof(T);
        if (eventTable.TryGetValue(type, out var existingDelegate))
        {
            eventTable[type] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            eventTable[type] = listener;
        }
    }

    public void Unsubscribe<T>(Action<T> listener) where T : IGameEvent
    {
        Type type = typeof(T);
        if (eventTable.TryGetValue(type, out var existingDelegate))
        {
            var newDelegate = Delegate.Remove(existingDelegate, listener);
            if (newDelegate == null)
            {
                eventTable.Remove(type);
            }
            else
            {
                eventTable[type] = newDelegate;
            }
        }
    }

    public void Publish<T>(T gameEvent) where T : IGameEvent
    {
        Type type = typeof(T);
        bool hasSubscribers = eventTable.TryGetValue(type, out var existingDelegate);
        if (!hasSubscribers)
        {
            Debug.LogWarning($"No subscribers for event type {type}. Event will be ignored.");
            return;
        }
        if (existingDelegate is Action<T> action)
        {
            action.Invoke(gameEvent);
        }
        else
        {
            Debug.LogError($"Event type {type} has subscribers, but they are of the wrong type. Expected Action<{type}>.");
        }
    }
}
