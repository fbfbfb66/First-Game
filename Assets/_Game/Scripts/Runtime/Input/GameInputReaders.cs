using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputReader : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 UINavigateInput { get; private set; }

    public event Action<Vector2> MoveChanged;
    public event Action JumpPressed;
    public event Action AttackPressed;
    public event Action DashPressed;
    public event Action InteractPressed;
    public event Action UseItemPressed;

    public event Action PausePressed;
    public event Action OpenInventoryPressed;
    public event Action OpenMapPressed;

    public event Action<Vector2> UINavigateChanged;
    public event Action UISubmitPressed;
    public event Action UICancelPressed;


    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        BindPlayerInput();
        BindGameInput();
        BindUIInput();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Game.Enable();
        inputActions.UI.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        inputActions.Game.Disable();
        inputActions.UI.Disable();
    }

    public void SetInputMode(GameLayerType layerType)
    {
        inputActions.Player.Disable();
        inputActions.Game.Disable();
        inputActions.UI.Disable();

        switch (layerType)
        {
            case GameLayerType.Gameplay:
                inputActions.Player.Enable();
                inputActions.Game.Enable();
                break;

            case GameLayerType.MainMenu:
            case GameLayerType.Inventory:
            case GameLayerType.Map:
            case GameLayerType.ServiceMenu:
            case GameLayerType.Pause:
            case GameLayerType.DialogueChoice:
                inputActions.UI.Enable();
                break;

            case GameLayerType.Dialogue:
                inputActions.Player.Enable();
                break;

            case GameLayerType.Cutscene:
                break;
        }
    }
    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    private void BindPlayerInput()
    {
        inputActions.Player.Move.performed += ctx =>
        {
            MoveInput = ctx.ReadValue<Vector2>();
            MoveChanged?.Invoke(MoveInput);
        };

        inputActions.Player.Move.canceled += ctx =>
        {
            MoveInput = Vector2.zero;
            MoveChanged?.Invoke(MoveInput);
        };

        inputActions.Player.Jump.performed += ctx => JumpPressed?.Invoke();
        inputActions.Player.Attack.performed += ctx => AttackPressed?.Invoke();
        inputActions.Player.Dash.performed += ctx => DashPressed?.Invoke();
        inputActions.Player.Interact.performed += ctx => InteractPressed?.Invoke();
        inputActions.Player.UseItem.performed += ctx => UseItemPressed?.Invoke();
    }

    private void BindGameInput()
    {
        inputActions.Game.Pause.performed += ctx => PausePressed?.Invoke();
        inputActions.Game.OpenInventory.performed += ctx => OpenInventoryPressed?.Invoke();
        inputActions.Game.OpenMap.performed += ctx => OpenMapPressed?.Invoke();
    }

    private void BindUIInput()
    {
        inputActions.UI.Navigate.performed += ctx =>
        {
            UINavigateInput = ctx.ReadValue<Vector2>();
            UINavigateChanged?.Invoke(UINavigateInput);
        };

        inputActions.UI.Navigate.canceled += ctx =>
        {
            UINavigateInput = Vector2.zero;
            UINavigateChanged?.Invoke(UINavigateInput);
        };

        inputActions.UI.Submit.performed += ctx => UISubmitPressed?.Invoke();
        inputActions.UI.Cancel.performed += ctx => UICancelPressed?.Invoke();
    }
}

