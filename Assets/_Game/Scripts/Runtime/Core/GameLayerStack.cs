using System;
using System.Collections.Generic;
using UnityEngine;

public class GameLayerStack : MonoBehaviour
{
    private readonly Stack<GameLayerType> layerStack = new();

    public GameLayerType CurrentLayer
    {
        get
        {
            if(layerStack.Count == 0)
            {
                return GameLayerType.None;
            }
            return layerStack.Peek();
        }
    }

    public event Action<GameLayerType,GameLayerType> CurrentLayerChanged;

    public void ResetTo(GameLayerType layer)
    {
        if(layer == GameLayerType.None)
        {
            Debug.LogWarning("Can not set None Layer");
            return;
        }
        GameLayerType previousLayer = CurrentLayer;
        layerStack.Clear();
        layerStack.Push(layer);

        NotifyLayerChanged(previousLayer,layer);
    }

    public void PushLayer(GameLayerType layer)
    {
        if(layer == GameLayerType.None)
        {
            Debug.LogWarning("Can not set None Layer");
            return;
        }
        if(ContainsLayer(layer))
        {
            Debug.LogWarning($"Layer {layer} is already here");
            return;
        }
        GameLayerType previousLayer = CurrentLayer;
        layerStack.Push(layer);
        NotifyLayerChanged(previousLayer,CurrentLayer);
    }

    public void PopLayer(GameLayerType layer)
    {
        if(layer == GameLayerType.None)
        {
            Debug.LogWarning("Can not set None Layer");
            return;
        }
        if(!IsCurrentLayer(layer))
        {
            Debug.LogWarning($"Layer {layer} is already the CurrentLayer");
            return;
        }
        GameLayerType previousLayer = CurrentLayer;
        layerStack.Pop();
        NotifyLayerChanged(previousLayer,CurrentLayer);     
    }

    public bool IsCurrentLayer(GameLayerType layer)
    {
        return CurrentLayer == layer;
    }
    public bool ContainsLayer(GameLayerType layer)
    {
        return layerStack.Contains(layer);
    }

    public GameLayerType[] GetActiveLayers()
    {
        return layerStack.ToArray();
    }
    private void NotifyLayerChanged(GameLayerType previousLayer,GameLayerType currentLayer)
    {
        Debug.Log($"Game layer changed: {previousLayer} -> {currentLayer}");
        CurrentLayerChanged?.Invoke(previousLayer,currentLayer);
    }
}
