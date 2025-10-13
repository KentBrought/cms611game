using UnityEngine;
using UnityEngine.UI;

public class SimpleLieSystem : MonoBehaviour
{
    public Button lieButton;
    public Button lieUpButton;
    public Button lieDownButton;
    public Button lieLeftButton;
    public Button lieRightButton;
    public Text liesCounterText;
    
    [Header("Settings")]
    public int maxLiesPerGame = 3;
    public int maxLiesPerTurn = 1;
    
    private int totalLiesUsed = 0;
    private int liesUsedThisTurn = 0;
    private bool isLieModeActive = false;
    private bool hasUsedLieThisTurn = false;
    private Vector2Int selectedLieDirection = Vector2Int.zero;
    
    private TurnManager turnManager;
    
    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
        
        // Set up button listeners
        if (lieButton != null)
        {
            lieButton.onClick.AddListener(OnLieButtonClicked);
            Debug.Log("Lie button listener added");
        }
        else
        {
            Debug.LogError("Lie button not assigned!");
        }
        
        if (lieUpButton != null)
            lieUpButton.onClick.AddListener(() => OnDirectionButtonClicked(Vector2Int.up));
        if (lieDownButton != null)
            lieDownButton.onClick.AddListener(() => OnDirectionButtonClicked(Vector2Int.down));
        if (lieLeftButton != null)
            lieLeftButton.onClick.AddListener(() => OnDirectionButtonClicked(Vector2Int.left));
        if (lieRightButton != null)
            lieRightButton.onClick.AddListener(() => OnDirectionButtonClicked(Vector2Int.right));
        
        // Hide everything initially
        HideAllUI();
        
        Debug.Log("SimpleLieSystem initialized");
    }
    
    void Update()
    {
        UpdateUIVisibility();
    }
    
    private void UpdateUIVisibility()
    {
        bool isRobberTurn = IsRobberTurn();
        
        if (!isRobberTurn)
        {
            // Hide everything for cop
            HideAllUI();
            return;
        }
        
        // Show lie counter for robber
        if (liesCounterText != null)
        {
            liesCounterText.gameObject.SetActive(true);
            liesCounterText.text = $"Lies: {totalLiesUsed}/{maxLiesPerGame}";
        }
        
        // Show lie button only if not in lie mode and can use lie
        bool hasMovementSteps = HasMovementStepsRemaining();
        if (lieButton != null)
        {
            bool canUseLie = CanUseLie();
            
            lieButton.gameObject.SetActive(!isLieModeActive && canUseLie);
            lieButton.interactable = canUseLie;
            
            // Debug logging
            if (!canUseLie && !hasMovementSteps)
            {
                Debug.Log("Lie button disabled - no movement steps remaining");
            }
        }
        
        // Show direction buttons only if in lie mode and has movement steps
        if (lieUpButton != null)
        {
            lieUpButton.gameObject.SetActive(isLieModeActive && hasMovementSteps);
            lieUpButton.interactable = hasMovementSteps;
        }
        if (lieDownButton != null)
        {
            lieDownButton.gameObject.SetActive(isLieModeActive && hasMovementSteps);
            lieDownButton.interactable = hasMovementSteps;
        }
        if (lieLeftButton != null)
        {
            lieLeftButton.gameObject.SetActive(isLieModeActive && hasMovementSteps);
            lieLeftButton.interactable = hasMovementSteps;
        }
        if (lieRightButton != null)
        {
            lieRightButton.gameObject.SetActive(isLieModeActive && hasMovementSteps);
            lieRightButton.interactable = hasMovementSteps;
        }
    }
    
    private bool IsRobberTurn()
    {
        if (turnManager != null)
        {
            return turnManager.GetActiveRole() == PlayerRole.Robber;
        }
        return false;
    }
    
    
    private bool HasMovementStepsRemaining()
    {
        if (turnManager != null)
        {
            return turnManager.CanMoveThisTurn();
        }
        return true; // Default to true if no turn manager
    }
    
    private void OnLieButtonClicked()
    {
        Debug.Log("LIE BUTTON CLICKED!");
        
        if (!CanUseLie())
        {
            Debug.Log("Cannot use lie");
            return;
        }
        
        isLieModeActive = true;
        Debug.Log("Lie mode activated - direction buttons should appear");
        
        // Force show direction buttons with detailed logging
        Debug.Log($"lieUpButton is null: {lieUpButton == null}");
        Debug.Log($"lieDownButton is null: {lieDownButton == null}");
        Debug.Log($"lieLeftButton is null: {lieLeftButton == null}");
        Debug.Log($"lieRightButton is null: {lieRightButton == null}");
        
        if (lieUpButton != null)
        {
            lieUpButton.gameObject.SetActive(true);
            Debug.Log("FORCED lieUpButton to active");
        }
        if (lieDownButton != null)
        {
            lieDownButton.gameObject.SetActive(true);
            Debug.Log("FORCED lieDownButton to active");
        }
        if (lieLeftButton != null)
        {
            lieLeftButton.gameObject.SetActive(true);
            Debug.Log("FORCED lieLeftButton to active");
        }
        if (lieRightButton != null)
        {
            lieRightButton.gameObject.SetActive(true);
            Debug.Log("FORCED lieRightButton to active");
        }
    }
    
    private void OnDirectionButtonClicked(Vector2Int direction)
    {
        Debug.Log($"Direction button clicked: {direction}");
        
        if (!isLieModeActive)
            return;
        
        // Store the selected lie direction for when the robber actually moves
        selectedLieDirection = direction;
        hasUsedLieThisTurn = true;
        liesUsedThisTurn++;
        totalLiesUsed++;
        
        isLieModeActive = false;
        
        Debug.Log($"Lie direction selected: {direction}. Next move will be fake. Total lies: {totalLiesUsed}/{maxLiesPerGame}");
    }
    
    private void HideAllUI()
    {
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
    
    public void OnTurnChanged()
    {
        liesUsedThisTurn = 0;
        hasUsedLieThisTurn = false;
        isLieModeActive = false;
        selectedLieDirection = Vector2Int.zero;
        Debug.Log("Turn changed - lie system reset");
    }
    
    [ContextMenu("Test Show Direction Buttons")]
    public void TestShowDirectionButtons()
    {
        Debug.Log("=== TESTING DIRECTION BUTTONS ===");
        Debug.Log($"lieUpButton: {(lieUpButton != null ? lieUpButton.name : "NULL")}");
        Debug.Log($"lieDownButton: {(lieDownButton != null ? lieDownButton.name : "NULL")}");
        Debug.Log($"lieLeftButton: {(lieLeftButton != null ? lieLeftButton.name : "NULL")}");
        Debug.Log($"lieRightButton: {(lieRightButton != null ? lieRightButton.name : "NULL")}");
        
        if (lieUpButton != null)
        {
            lieUpButton.gameObject.SetActive(true);
            Debug.Log("TEST: lieUpButton activated");
        }
        if (lieDownButton != null)
        {
            lieDownButton.gameObject.SetActive(true);
            Debug.Log("TEST: lieDownButton activated");
        }
        if (lieLeftButton != null)
        {
            lieLeftButton.gameObject.SetActive(true);
            Debug.Log("TEST: lieLeftButton activated");
        }
        if (lieRightButton != null)
        {
            lieRightButton.gameObject.SetActive(true);
            Debug.Log("TEST: lieRightButton activated");
        }
    }
    
    public bool HasActiveLie()
    {
        return hasUsedLieThisTurn && selectedLieDirection != Vector2Int.zero;
    }
    
    public bool CanUseLie()
    {
        bool hasLiesLeft = totalLiesUsed < maxLiesPerGame && liesUsedThisTurn < maxLiesPerTurn && !hasUsedLieThisTurn;
        bool hasMovementSteps = HasMovementStepsRemaining();
        
        return hasLiesLeft && hasMovementSteps;
    }
    
    public bool IsInLieMode()
    {
        return isLieModeActive;
    }
    
    public Vector2Int GetSelectedLieDirection()
    {
        return selectedLieDirection;
    }
    
    public void ConsumeActiveLie()
    {
        // Record the fake move when the robber actually moves
        if (selectedLieDirection != Vector2Int.zero)
        {
            RecordFakeMove(selectedLieDirection);
        }
        
        // Reset the lie state
        selectedLieDirection = Vector2Int.zero;
        hasUsedLieThisTurn = false;
    }
    
    private void RecordFakeMove(Vector2Int fakeDirection)
    {
        // Convert Vector2Int to MoveDirection
        MoveDirection fakeMoveDirection = ConvertVector2IntToMoveDirection(fakeDirection);
        
        // Record the fake move in the MoveTracker
        if (MoveTracker.Instance != null)
        {
            MoveTracker.Instance.RecordFakeMove(PlayerRole.Robber, fakeMoveDirection);
            Debug.Log($"Recorded fake move: {fakeMoveDirection} for Robber");
        }
        else
        {
            Debug.LogError("MoveTracker.Instance is null!");
        }
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
}
