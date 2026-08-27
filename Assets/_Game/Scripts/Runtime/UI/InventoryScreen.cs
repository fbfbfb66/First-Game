using UnityEngine;

public class InventoryScreen : MonoBehaviour
{
    [SerializeField] private GameLayerStack layerStack;
    [SerializeField] private GameObject root;

    private void Awake()
    {
        if(layerStack == null)
            layerStack = FindAnyObjectByType<GameLayerStack>();
    }

    private void OnEnable()
    {
        if(layerStack != null)
        {
            layerStack.CurrentLayerChanged += CurrentLayerChanged;
        }
    }

    private void OnDisable()
    {
        if(layerStack != null)
        {
            layerStack.CurrentLayerChanged -= CurrentLayerChanged;
        }
    }

    private void CurrentLayerChanged(GameLayerType previous,GameLayerType current)
    {
        if(current == GameLayerType.Inventory)
        {
            root.SetActive(true);
        }
        else
        {
            root.SetActive(false);
        }
    }
}
