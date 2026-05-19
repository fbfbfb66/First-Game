using UnityEngine;

public class InputRouter : MonoBehaviour
{
    [SerializeField] private GameInputReader inputReader;
    [SerializeField] private GameLayerStack gameLayerStack;
    [SerializeField] private PlayerControlArbitration playerControlArbitration;
    [SerializeField] private PlayerInputReceiver playerInputReceiver;
    [SerializeField] private DialogueManager dialogueManager;

    private void Awake()
    {
        if (inputReader == null)
        {
            inputReader = FindAnyObjectByType<GameInputReader>();
        }

        if (gameLayerStack == null)
        {
            gameLayerStack = FindAnyObjectByType<GameLayerStack>();
        }

        if (playerControlArbitration == null)
        {
            playerControlArbitration = FindAnyObjectByType<PlayerControlArbitration>();
        }
        if(playerInputReceiver == null)
        {
            playerInputReceiver = FindAnyObjectByType<PlayerInputReceiver>();
        }
        if(dialogueManager == null)
        {
            dialogueManager = GetComponent<DialogueManager>();
        }
    }

    private void OnEnable()
    {
        if(inputReader == null)
        {
            Debug.LogWarning("Not Got inputReader");
            return;
        }
        if(gameLayerStack != null)
            gameLayerStack.CurrentLayerChanged += OnCurrentLayerChanged;

        inputReader.MoveChanged += OnMoveChanged;
        inputReader.JumpPressed += OnJumpPressed;
        inputReader.AttackPressed += OnAttackPressed;
        inputReader.DashPressed += OnDashPressed;
        inputReader.InteractPressed += OnInteractPressed;
        inputReader.UseItemPressed += OnUseItemPressed;

        inputReader.PausePressed += OnPausePressed;
        inputReader.OpenInventoryPressed += OnOpenInventoryPressed;
        inputReader.OpenMapPressed += OnOpenMapPressed;

        inputReader.UINavigateChanged += OnUINavigateChanged;
        inputReader.UISubmitPressed += OnUISubmitPressed;
        inputReader.UICancelPressed += OnUICancelPressed;
    }

    private void OnDisable()
    {
        if(gameLayerStack != null)
            gameLayerStack.CurrentLayerChanged -= OnCurrentLayerChanged;

        inputReader.MoveChanged -= OnMoveChanged;
        inputReader.JumpPressed -= OnJumpPressed;
        inputReader.AttackPressed -= OnAttackPressed;
        inputReader.DashPressed -= OnDashPressed;
        inputReader.InteractPressed -= OnInteractPressed;
        inputReader.UseItemPressed -= OnUseItemPressed;

        inputReader.PausePressed -= OnPausePressed;
        inputReader.OpenInventoryPressed -= OnOpenInventoryPressed;
        inputReader.OpenMapPressed -= OnOpenMapPressed;

        inputReader.UINavigateChanged -= OnUINavigateChanged;
        inputReader.UISubmitPressed -= OnUISubmitPressed;
        inputReader.UICancelPressed -= OnUICancelPressed;
    }

    private void OnCurrentLayerChanged(GameLayerType previousLayer, GameLayerType currentLayer)
    {   
        inputReader.SetInputMode(currentLayer);
    }

    private void OnMoveChanged(Vector2 moveInput)
    {
        if (!IsCurrentLayer(GameLayerType.Gameplay))
        {
            return;
        }

        if (!playerControlArbitration.CanMove)
        {
            return;
        }

        playerInputReceiver.SetMoveInput(moveInput);
    }

    private void OnJumpPressed()
    {
        if (!IsCurrentLayer(GameLayerType.Gameplay))
        {
            return;
        }

        if (!playerControlArbitration.CanJump)
        {
            return;
        }

        playerInputReceiver.RequestJump();
    }

    private void OnAttackPressed()
    {
        if (!IsCurrentLayer(GameLayerType.Gameplay))
        {
            return;
        }

        if (!playerControlArbitration.CanAttack)
        {
            return;
        }

        playerInputReceiver.RequestAttack();
    }

    private void OnDashPressed()
    {
        if (!IsCurrentLayer(GameLayerType.Gameplay))
        {
            return;
        }

        if (!playerControlArbitration.CanDash)
        {
            return;
        }

        playerInputReceiver.RequestDash();
    }

    private void OnInteractPressed()
    {
        if (gameLayerStack == null)
        {
            return;
        }

        switch (gameLayerStack.CurrentLayer)
        {
            case GameLayerType.Gameplay:
                if (playerControlArbitration.CanWorldInteract)
                {
                    playerInputReceiver.RequestWorldInteract();
                }
                break;

            case GameLayerType.Dialogue:
                dialogueManager.RequestAdvance();
                break;
                
            default:
                Debug.Log($"Interact ignored in layer: {gameLayerStack.CurrentLayer}");
                break;
        }
    }

    private void OnUseItemPressed()
    {
        if (!IsCurrentLayer(GameLayerType.Gameplay))
        {
            return;
        }

        if (!playerControlArbitration.CanUseItem)
        {
            return;
        }

        Debug.Log("Route Use Item to Player.");
    }

    private void OnPausePressed()
    {
        if (gameLayerStack == null)
        {
            return;
        }

        if (gameLayerStack.IsCurrentLayer(GameLayerType.Gameplay))
        {
            gameLayerStack.PushLayer(GameLayerType.Pause);
            Debug.Log("Pause opened.");
            return;
        }

        if (gameLayerStack.IsCurrentLayer(GameLayerType.Pause))
        {
            gameLayerStack.PopLayer(GameLayerType.Pause);
            Debug.Log("Pause closed.");
        }
    }

    private void OnOpenInventoryPressed()
    {
        if (gameLayerStack == null)
        {
            return;
        }

        if (gameLayerStack.IsCurrentLayer(GameLayerType.Gameplay))
        {
            gameLayerStack.PushLayer(GameLayerType.Inventory);
            Debug.Log("Inventory opened.");
            return;
        }

        if (gameLayerStack.IsCurrentLayer(GameLayerType.Inventory))
        {
            gameLayerStack.PopLayer(GameLayerType.Inventory);
            Debug.Log("Inventory closed.");
        }
    }

    private void OnOpenMapPressed()
    {
        if (gameLayerStack == null)
        {
            return;
        }

        if (gameLayerStack.IsCurrentLayer(GameLayerType.Gameplay))
        {
            gameLayerStack.PushLayer(GameLayerType.Map);
            Debug.Log("Map opened.");
            return;
        }

        if (gameLayerStack.IsCurrentLayer(GameLayerType.Map))
        {
            gameLayerStack.PopLayer(GameLayerType.Map);
            Debug.Log("Map closed.");
        }
    }

    private void OnUINavigateChanged(Vector2 navigateInput)
    {
        switch (gameLayerStack.CurrentLayer)
        {
            case GameLayerType.DialogueChoice:
                if (dialogueManager != null)
                {
                    dialogueManager.HandleChoiceSelectedNavigate(navigateInput);
                }
                break;

            case GameLayerType.Inventory:
                Debug.Log($"Route Navigate to inventory: {navigateInput}");
                break;

            case GameLayerType.Map:
                Debug.Log($"Route Navigate to map: {navigateInput}");
                break;

            case GameLayerType.ServiceMenu:
                Debug.Log($"Route Navigate to service menu: {navigateInput}");
                break;

            case GameLayerType.Pause:
                Debug.Log($"Route Navigate to pause menu: {navigateInput}");
                break;
            case GameLayerType.MainMenu:
                Debug.Log($"Route Navigate to Main menu: {navigateInput}");
                break;
        }
    }

    private void OnUISubmitPressed()
    {
        switch (gameLayerStack.CurrentLayer)
        {
            case GameLayerType.DialogueChoice:
                if (dialogueManager != null)
                {
                    dialogueManager.HandleChoiceConfirmed();
                }
                break;

            case GameLayerType.Inventory:
                Debug.Log("Route Submit to inventory.");
                break;

            case GameLayerType.Map:
                Debug.Log("Route Submit to map.");
                break;

            case GameLayerType.ServiceMenu:
                Debug.Log("Route Submit to service menu.");
                break;

            case GameLayerType.Pause:
                Debug.Log("Route Submit to pause menu.");
                break;
        }
    }

    private void OnUICancelPressed()
    {
        if (gameLayerStack == null)
        {
            return;
        }

        switch (gameLayerStack.CurrentLayer)
        {
            case GameLayerType.Inventory:
                gameLayerStack.PopLayer(GameLayerType.Inventory);
                Debug.Log("Inventory closed by cancel.");
                break;

            case GameLayerType.Map:
                gameLayerStack.PopLayer(GameLayerType.Map);
                Debug.Log("Map closed by cancel.");
                break;

            case GameLayerType.ServiceMenu:
                gameLayerStack.PopLayer(GameLayerType.ServiceMenu);
                Debug.Log("Service menu closed by cancel.");
                break;

            case GameLayerType.Pause:
                gameLayerStack.PopLayer(GameLayerType.Pause);
                Debug.Log("Pause closed by cancel.");
                break;

            case GameLayerType.DialogueChoice:
                Debug.Log("Cancel dialogue choice.");
                break;
        }
    }

    private bool IsCurrentLayer(GameLayerType layerType)
    {
        return gameLayerStack != null
            && gameLayerStack.IsCurrentLayer(layerType);
    }
}