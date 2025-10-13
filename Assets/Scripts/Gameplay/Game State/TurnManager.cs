using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public Text turnCounterText; 
    public Text coinCounterText;
    public Text movementStepsText;
    public Text previousMovesText;
    public Button nextTurnButton;
    
    [Header("Lie System UI References")]
    public Button lieButton;
    public Button lieUpButton;
    public Button lieDownButton;
    public Button lieLeftButton;
    public Button lieRightButton;
    public Text liesCounterText; 
    private int turnCount = 1;
    private int currentMovementSteps = 0;
    private int maxMovementSteps = 0;
    private PlayerRole activeRole = PlayerRole.Robber;
    
    private SimpleLieSystem simpleLieSystem;

    void Start()
    {
        simpleLieSystem = FindFirstObjectByType<SimpleLieSystem>();
        
        // Initialize simple lie system with UI references
        if (simpleLieSystem != null)
        {
            simpleLieSystem.lieButton = lieButton;
            simpleLieSystem.lieUpButton = lieUpButton;
            simpleLieSystem.lieDownButton = lieDownButton;
            simpleLieSystem.lieLeftButton = lieLeftButton;
            simpleLieSystem.lieRightButton = lieRightButton;
            simpleLieSystem.liesCounterText = liesCounterText;
        }
        
        // Hide all lie UI elements initially
        HideAllLieUI();
        
        UpdateTurnCounter();
        UpdateCoinDisplay(0);
        GenerateNewMovementSteps();
        if (nextTurnButton != null) nextTurnButton.interactable = false;
        nextTurnButton.onClick.AddListener(NextTurn);
    }

    public bool CanMoveThisTurn()
    {
        return currentMovementSteps < maxMovementSteps;
    }
    
    public int GetCurrentMovementSteps()
    {
        return currentMovementSteps;
    }
    
    public int GetMaxMovementSteps()
    {
        return maxMovementSteps;
    }

    public void CharacterMoved()
    {
        if (currentMovementSteps < maxMovementSteps)
        {
            currentMovementSteps++;
            UpdateMovementStepsDisplay();
            
            if (currentMovementSteps >= maxMovementSteps && nextTurnButton != null)
            {
                nextTurnButton.interactable = true;
            }
        }
    }

    private void NextTurn()
    {
        if (currentMovementSteps < maxMovementSteps)
        {
            return;
        }
        
        turnCount++;
        GenerateNewMovementSteps();
        UpdateTurnCounter();
        
        activeRole = (activeRole == PlayerRole.Robber) ? PlayerRole.Cop : PlayerRole.Robber;
        DisplayPreviousPlayerMoves();
        ToggleActivePlayerVisibility();
        UpdateTreasureVisibility();
        
        // Notify lie system of turn change
        if (simpleLieSystem != null)
        {
            simpleLieSystem.OnTurnChanged();
        }
        
        GameStateManager gameState = FindFirstObjectByType<GameStateManager>();
        if (gameState != null)
        {
            gameState.IncrementRound();
        }
        
        GameSceneManager.Instance.LoadTransitionScreen();
        if (nextTurnButton != null) nextTurnButton.interactable = false;
    }
    
    public void InitializeIndicatorVisibility()
    {
        // This method should be called after indicators are created to set initial visibility
        ToggleIndicatorVisibility();
    }

    private void UpdateTurnCounter()
    {
        turnCounterText.text = "Round: " + (turnCount/2 + 1);
    }
    
    public void UpdateCoinDisplay(int coinCount)
    {
        if (coinCounterText != null)
        {
            coinCounterText.text = "Coins: " + coinCount;
        }
    }
    
    private void GenerateNewMovementSteps()
    {
        maxMovementSteps = Random.Range(1, 5);
        currentMovementSteps = 0;
        UpdateMovementStepsDisplay();
        if (nextTurnButton != null) nextTurnButton.interactable = false;
    }
    
    private void UpdateMovementStepsDisplay()
    {
        if (movementStepsText != null)
        {
            movementStepsText.text = $"Steps: {currentMovementSteps}/{maxMovementSteps}";
        }
    }
    
    public void ContinueAfterTransition()
    {
        UpdateTreasureVisibility();
        GameSceneManager.Instance.ContinueToNextPlayer();
    }

    public PlayerRole GetActiveRole()
    {
        return activeRole;
    }

    private void ToggleActivePlayerVisibility()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController p in players)
        {
            var sr = p.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = (p.GetRole() == activeRole);
            }
        }
        
        // Also toggle indicator visibility
        ToggleIndicatorVisibility();
    }
    
    private void ToggleIndicatorVisibility()
    {
        PlayerIndicatorController[] indicators = FindObjectsByType<PlayerIndicatorController>(FindObjectsSortMode.None);
        foreach (PlayerIndicatorController indicator in indicators)
        {
            var sr = indicator.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = (indicator.GetIndicatorRole() == activeRole);
            }
        }
    }
    
    private void DisplayPreviousPlayerMoves()
    {
        if (MoveTracker.Instance != null)
        {
            MoveTracker.Instance.DisplayPreviousPlayerMoves(activeRole);
            
            PlayerRole previousRole = (activeRole == PlayerRole.Robber) ? PlayerRole.Cop : PlayerRole.Robber;
            MoveTracker.Instance.ClearMovesForRole(previousRole);
        }
    }
    
    private void UpdateTreasureVisibility()
    {
        TreasureController[] treasures = FindObjectsByType<TreasureController>(FindObjectsSortMode.None);
        foreach (TreasureController treasure in treasures)
        {
            if (treasure != null)
            {
                treasure.UpdateVisibilityForCurrentTurn();
            }
        }
    }
    
    public MoveDirection GetFakeMoveForCop()
    {
        if (simpleLieSystem != null && simpleLieSystem.HasActiveLie())
        {
            Vector2Int lieDirection = simpleLieSystem.GetSelectedLieDirection();
            return ConvertVector2IntToMoveDirection(lieDirection);
        }
        return MoveDirection.Up; // Default fallback
    }
    
    private MoveDirection ConvertVector2IntToMoveDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
            return MoveDirection.Up;
        else if (direction == Vector2Int.down)
            return MoveDirection.Down;
        else if (direction == Vector2Int.left)
            return MoveDirection.Left;
        else if (direction == Vector2Int.right)
            return MoveDirection.Right;
        else
            return MoveDirection.Up; // Default fallback
    }
    
    public void ConsumeActiveLie()
    {
        if (simpleLieSystem != null)
        {
            simpleLieSystem.ConsumeActiveLie();
        }
    }
    
    void Update()
    {
        // Update lie UI visibility based on current role
        UpdateLieUIVisibility();
    }
    
    private void HideAllLieUI()
    {
        // Hide all lie-related UI elements
        if (lieButton != null)
            lieButton.gameObject.SetActive(false);
        if (liesCounterText != null)
            liesCounterText.gameObject.SetActive(false);
        if (lieUpButton != null)
            lieUpButton.gameObject.SetActive(false);
        if (lieDownButton != null)
            lieDownButton.gameObject.SetActive(false);
        if (lieLeftButton != null)
            lieLeftButton.gameObject.SetActive(false);
        if (lieRightButton != null)
            lieRightButton.gameObject.SetActive(false);
    }
    
    private void UpdateLieUIVisibility()
    {
        bool isRobberTurn = (activeRole == PlayerRole.Robber);
        
        // Hide all lie UI for cop
        if (!isRobberTurn)
        {
            HideAllLieUI();
            return;
        }
        
        // For robber, show lie counter and potentially lie button
        if (liesCounterText != null)
            liesCounterText.gameObject.SetActive(true);
            
        // Show lie button for robber (unless in lie mode)
        if (simpleLieSystem != null)
        {
            bool isInLieMode = simpleLieSystem.IsInLieMode();
            
            // Show lie button for robber unless in lie mode
            if (lieButton != null)
            {
                lieButton.gameObject.SetActive(!isInLieMode);
                // Make button interactable only if robber can use a lie
                lieButton.interactable = simpleLieSystem.CanUseLie();
            }
                
            // Show direction buttons only if in lie mode
            if (isInLieMode)
            {
                Debug.Log("TurnManager: Showing direction buttons (in lie mode)");
                if (lieUpButton != null)
                    lieUpButton.gameObject.SetActive(true);
                if (lieDownButton != null)
                    lieDownButton.gameObject.SetActive(true);
                if (lieLeftButton != null)
                    lieLeftButton.gameObject.SetActive(true);
                if (lieRightButton != null)
                    lieRightButton.gameObject.SetActive(true);
            }
            else
            {
                if (lieUpButton != null)
                    lieUpButton.gameObject.SetActive(false);
                if (lieDownButton != null)
                    lieDownButton.gameObject.SetActive(false);
                if (lieLeftButton != null)
                    lieLeftButton.gameObject.SetActive(false);
                if (lieRightButton != null)
                    lieRightButton.gameObject.SetActive(false);
            }
        }
        else
        {
            // Fallback: show lie button for robber if no lie system manager
            if (lieButton != null)
            {
                lieButton.gameObject.SetActive(true);
                lieButton.interactable = true;
            }
        }
    }
}