using UnityEngine;

public class InputRouter : MonoBehaviour
{
    [SerializeField] private ActionCommandKey backDodgeCommand;
    [SerializeField] private ActionCommandKey jumpCommand;
    [SerializeField] private ActionCommandKey interactCommand;
    [SerializeField] private ActionCommandKey diveAttackCommand;
    [SerializeField] private ActionCommandKey attackCommand;
    [SerializeField] private ActionCommandKey dashCommand;
    [SerializeField] private ActionCommandKey launcherAttackCommand;
    [SerializeField] private GameInputReader inputReader;
    [SerializeField] private GameLayerStack gameLayerStack;
    [SerializeField] private PlayerControlArbitration playerControlArbitration;
    [SerializeField] private InventoryView inventoryView;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Player player;
    private PlayerCombatActionController playerCombatActionController => player != null ? player.CombatActionController : null;
    private PlayerMotionController playerMotionController => player != null ? player.MotionController : null;


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
        if (player == null)
            player = FindAnyObjectByType<Player>();
        if (player == null)
            Debug.LogWarning($"InputRouter {name} has no Player; movement input will be ignored.", this);
        if(dialogueManager == null)
        {
            dialogueManager = GetComponent<DialogueManager>();
        }
        if(inventoryView == null)
        {
            inventoryView = FindAnyObjectByType<InventoryView>(FindObjectsInactive.Include);
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
        inputReader.LauncherAttackPressed += OnLauncherAttackPressed;
        inputReader.DiveAttackHeld += OnDiveAttackHeld;

        inputReader.PausePressed += OnPausePressed;
        inputReader.OpenInventoryPressed += OnOpenInventoryPressed;
        inputReader.OpenMapPressed += OnOpenMapPressed;

        inputReader.UINavigateChanged += OnUINavigateChanged;
        inputReader.UISubmitPressed += OnUISubmitPressed;
        inputReader.UICancelPressed += OnUICancelPressed;
        inputReader.UIRotateItemPressed += OnUIRotateItemPressed;
        inputReader.UseItemInInventoryPressed += OnUsingItemInInventory;
        inputReader.DropItemInInventoryPressed += OnDropItemInInventory;
    }

    private void OnDisable()
    {
        if (player != null) player.CommandInput.ClearMoveInput();
        if (inputReader == null) return;
        if(gameLayerStack != null)
            gameLayerStack.CurrentLayerChanged -= OnCurrentLayerChanged;

        inputReader.MoveChanged -= OnMoveChanged;
        inputReader.JumpPressed -= OnJumpPressed;
        inputReader.AttackPressed -= OnAttackPressed;
        inputReader.DashPressed -= OnDashPressed;
        inputReader.InteractPressed -= OnInteractPressed;
        inputReader.UseItemPressed -= OnUseItemPressed;
        inputReader.LauncherAttackPressed -= OnLauncherAttackPressed;
        inputReader.DiveAttackHeld -= OnDiveAttackHeld;

        inputReader.PausePressed -= OnPausePressed;
        inputReader.OpenInventoryPressed -= OnOpenInventoryPressed;
        inputReader.OpenMapPressed -= OnOpenMapPressed;

        inputReader.UINavigateChanged -= OnUINavigateChanged;
        inputReader.UISubmitPressed -= OnUISubmitPressed;
        inputReader.UICancelPressed -= OnUICancelPressed;
        inputReader.UIRotateItemPressed -= OnUIRotateItemPressed;
        inputReader.UseItemInInventoryPressed -= OnUsingItemInInventory;
        inputReader.DropItemInInventoryPressed -= OnDropItemInInventory;
    }

    private void OnDropItemInInventory()
    {
        if (!IsCurrentLayer(GameLayerType.Inventory)) return;
        if (inventoryView == null)
        {
            Debug.LogWarning("InputRouter cannot drop inventory item: InventoryView is not assigned.", this);
            return;
        }
        inventoryView.DropItem();
    }

    private void OnUsingItemInInventory()
    {
        if (!IsCurrentLayer(GameLayerType.Inventory)) return;
        if (inventoryView == null)
        {
            Debug.LogWarning("inventoryView not set");
            return;
        }
        inventoryView.UseItem();
    }

    private void OnDiveAttackHeld()
    {
        if(playerCombatActionController == null)
        {
            Debug.LogWarning("Missed playerCombatActionController");
            return;
        }

        if (!IsCurrentLayer(GameLayerType.Gameplay))
        {
            return;
        }

        if (!playerControlArbitration.CanAttack)
        {
            return;
        }
        playerCombatActionController.RequestAction(diveAttackCommand);
    }

    private void OnLauncherAttackPressed()
    {
        if(playerCombatActionController == null)
        {
            Debug.LogWarning("Missed playerCombatActionController");
            return;
        }

        if (!IsCurrentLayer(GameLayerType.Gameplay))
        {
            return;
        }

        if (!playerControlArbitration.CanAttack)
        {
            return;
        }

        playerCombatActionController.RequestAction(launcherAttackCommand);
    }

    private void OnCurrentLayerChanged(GameLayerType previousLayer, GameLayerType currentLayer)
    {   
        if (player != null && (currentLayer != GameLayerType.Gameplay || playerControlArbitration == null || !playerControlArbitration.CanMove))
            player.CommandInput.ClearMoveInput();
        inputReader.SetInputMode(currentLayer);
    }

    private void Update()
    {
        if (player == null || player.CommandInput.MoveInput == Vector2.zero) return;
        if (!IsCurrentLayer(GameLayerType.Gameplay) || playerControlArbitration == null || !playerControlArbitration.CanMove)
            player.CommandInput.ClearMoveInput();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && player != null) player.CommandInput.ClearMoveInput();
    }

    private void OnMoveChanged(Vector2 moveInput)
    {
        if (player == null) return;
        if (!IsCurrentLayer(GameLayerType.Gameplay) || playerControlArbitration == null || !playerControlArbitration.CanMove)
        {
            player.CommandInput.ClearMoveInput();
            return;
        }

        if (playerMotionController == null)
        {
            Debug.LogWarning("Missed actionController");
            return;
        }
        player.CommandInput.SetMoveInput(moveInput);
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

        if (playerMotionController == null)
        {
            Debug.LogWarning($"InputRouter {name} has no ActionController; Jump request ignored.", this);
            return;
        }
        Vector2 input = player.CommandInput.MoveInput;
        if(input.y < 0)
        {
            playerMotionController.RequestAction(backDodgeCommand);
            return;
        }
        playerMotionController.RequestAction(jumpCommand);
    }

    private void OnAttackPressed()
    {
        if(playerCombatActionController == null)
        {
            Debug.LogWarning("Missed playerCombatActionController");
            return;
        }

        if (!IsCurrentLayer(GameLayerType.Gameplay))
        {
            return;
        }

        if (!playerControlArbitration.CanAttack)
        {
            return;
        }

        playerCombatActionController.RequestAction(attackCommand);
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
        if (playerMotionController == null)
        {
            Debug.LogWarning($"InputRouter {name} has no ActionController; Dash request ignored.", this);
            return;
        }
        playerMotionController.RequestAction(dashCommand);
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
                    if (playerMotionController == null)
                    {
                        Debug.LogWarning($"InputRouter {name} has no MotionController; world interaction request ignored.", this);
                        return;
                    }
                    playerMotionController.RequestAction(interactCommand);
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

        if (IsCurrentLayer(GameLayerType.Gameplay))
        {
            gameLayerStack.PushLayer(GameLayerType.Pause);
            Debug.Log("Pause opened.");
            return;
        }

        if (IsCurrentLayer(GameLayerType.Pause))
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

        if (IsCurrentLayer(GameLayerType.Gameplay))
        {
            gameLayerStack.PushLayer(GameLayerType.Inventory);
            Debug.Log("Inventory opened.");
            return;
        }

        if (IsCurrentLayer(GameLayerType.Inventory))
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

        if (IsCurrentLayer(GameLayerType.Gameplay))
        {
            gameLayerStack.PushLayer(GameLayerType.Map);
            Debug.Log("Map opened.");
            return;
        }

        if (IsCurrentLayer(GameLayerType.Map))
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

    private void OnUIRotateItemPressed()
    {
        if (gameLayerStack == null) return;

        if (IsCurrentLayer(GameLayerType.Inventory))
        {
            if(inventoryView != null)
                inventoryView.RotateDragItem();
        }
    }

    private bool IsCurrentLayer(GameLayerType layerType)
    {
        return gameLayerStack != null
            && gameLayerStack.IsCurrentLayer(layerType);
    }
}
