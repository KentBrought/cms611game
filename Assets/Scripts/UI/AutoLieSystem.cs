using UnityEngine;
using UnityEngine.UI;

public class AutoLieSystem : MonoBehaviour
{
    [Header("Settings")]
    public int maxLiesPerGame = 3;
    public int maxLiesPerTurn = 1;
    
    private int totalLiesUsed = 0;
    private int liesUsedThisTurn = 0;
    private bool isLieModeActive = false;
    private bool hasUsedLieThisTurn = false;
    private Vector2Int selectedLieDirection = Vector2Int.zero;
    
    private TurnManager turnManager;
    private Button lieButton;
    private Button lieUpButton;
    private Button lieDownButton;
    private Button lieLeftButton;
    private Button lieRightButton;
    private Text liesCounterText;
    
    void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();
        
        // Auto-find UI elements by name
        FindUIElements();
        
        // Set up button listeners
        SetupButtonListeners();
        
        Debug.Log("AutoLieSystem initialized");
    }
    
    private void FindUIElements()
    {
        // Find buttons by name
        GameObject lieButtonObj = GameObject.Find("LieButton");
        if (lieButtonObj != null)
        {
            lieButton = lieButtonObj.GetComponent<Button>();
            Debug.Log($"Found LieButton: {lieButton != null}");
        }
        
        GameObject lieUpButtonObj = GameObject.Find("LieUpButton");
        if (lieUpButtonObj != null)
        {
            lieUpButton = lieUpButtonObj.GetComponent<Button>();
            Debug.Log($"Found LieUpButton: {lieUpButton != null}");
        }
        
        GameObject lieDownButtonObj = GameObject.Find("LieDownButton");
        if (lieDownButtonObj != null)
        {
            lieDownButton = lieDownButtonObj.GetComponent<Button>();
            Debug.Log($"Found LieDownButton: {lieDownButton != null}");
        }
        
        GameObject lieLeftButtonObj = GameObject.Find("LieLeftButton");
        if (lieLeftButtonObj != null)
        {
            lieLeftButton = lieLeftButtonObj.GetComponent<Button>();
            Debug.Log($"Found LieLeftButton: {lieLeftButton != null}");
        }
        
        GameObject lieRightButtonObj = GameObject.Find("LieRightButton");
        if (lieRightButtonObj != null)
        {
            lieRightButton = lieRightButtonObj.GetComponent<Button>();
            Debug.Log($"Found LieRightButton: {lieRightButton != null}");
        }
        
        GameObject liesCounterObj = GameObject.Find("LiesCounter");
        if (liesCounterObj != null)
        {
            liesCounterText = liesCounterObj.GetComponent<Text>();
            Debug.Log($"Found LiesCounter: {liesCounterText != null}");
        }
    }
    
    private void SetupButtonListeners()
    {
        if (lieButton != null)
        {
            lieButton.onClick.AddListener(OnLieButtonClicked);
            Debug.Log("Lie button listener added");
        }
        else
        {
            Debug.LogError("LieButton not found!");
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
        if (lieButton != null)
        {
            bool canUseLie = CanUseLie();
            bool hasMovementSteps = HasMovementStepsRemaining();
            
            lieButton.gameObject.SetActive(!isLieModeActive && canUseLie);
            lieButton.interactable = canUseLie;
            
            // Debug logging
            if (!canUseLie && !hasMovementSteps)
            {
                Debug.Log("Lie button disabled - no movement steps remaining");
            }
        }
        
        // Show direction buttons only if in lie mode
        if (lieUpButton != null)
            lieUpButton.gameObject.SetActive(isLieModeActive);
        if (lieDownButton != null)
            lieDownButton.gameObject.SetActive(isLieModeActive);
        if (lieLeftButton != null)
            lieLeftButton.gameObject.SetActive(isLieModeActive);
        if (lieRightButton != null)
            lieRightButton.gameObject.SetActive(isLieModeActive);
    }
    
    private bool IsRobberTurn()
    {
        if (turnManager != null)
        {
            return turnManager.GetActiveRole() == PlayerRole.Robber;
        }
        return false;
    }
    
    private bool CanUseLie()
    {
        bool hasLiesLeft = totalLiesUsed < maxLiesPerGame && liesUsedThisTurn < maxLiesPerTurn && !hasUsedLieThisTurn;
        bool hasMovementSteps = HasMovementStepsRemaining();
        
        return hasLiesLeft && hasMovementSteps;
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
        
        // Force show direction buttons
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
    
    public bool HasActiveLie()
    {
        return hasUsedLieThisTurn && selectedLieDirection != Vector2Int.zero;
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
}
