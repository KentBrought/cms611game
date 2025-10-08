using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public Text turnCounterText; 
    public Text coinCounterText;  // New UI text for coin display
    public Text movementStepsText;  // New UI text for movement steps display
    public Text previousMovesText;  // New UI text for displaying previous player moves
    public Button nextTurnButton; 
    private int turnCount = 1;
    private int currentMovementSteps = 0;
    private int maxMovementSteps = 0;
    private PlayerRole activeRole = PlayerRole.Robber; // start with robber by default

    void Start()
    {
        UpdateTurnCounter();
        UpdateCoinDisplay(0);  // Initialize coin display
        GenerateNewMovementSteps();  // Generate initial movement steps
        if (nextTurnButton != null) nextTurnButton.interactable = false; // cannot advance until steps are used
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
            Debug.Log($"Character moved this turn. Steps used: {currentMovementSteps}/{maxMovementSteps}");
            // Enable next turn button once steps are exhausted
            if (currentMovementSteps >= maxMovementSteps && nextTurnButton != null)
            {
                nextTurnButton.interactable = true;
            }
        }
        else
        {
            Debug.Log("Character has used all movement steps this turn.");
        }
    }

    private void NextTurn()
    {
        // Disallow early next turn if steps remain
        if (currentMovementSteps < maxMovementSteps)
        {
            Debug.Log("Cannot end turn: steps remain.");
            return;
        }
        turnCount++;
        GenerateNewMovementSteps();
        UpdateTurnCounter();
        Debug.Log($"Next turn started. Movement steps available: {maxMovementSteps}");
        // Switch active role for pass-and-play
        activeRole = (activeRole == PlayerRole.Robber) ? PlayerRole.Cop : PlayerRole.Robber;
        // Display previous player's moves
        DisplayPreviousPlayerMoves();
        // Toggle visibility to only show active player
        ToggleActivePlayerVisibility();
        // Notify GameStateManager of round increment
        GameStateManager gameState = FindFirstObjectByType<GameStateManager>();
        if (gameState != null)
        {
            gameState.IncrementRound();
        }
        // Show transition scene so players can hand off
        GameSceneManager.Instance.LoadTransitionScreen();
        if (nextTurnButton != null) nextTurnButton.interactable = false; // reset until new steps are used
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
        maxMovementSteps = Random.Range(1, 5); // Random number between 1-4 (inclusive)
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
    
    // Called from Transition screen button when ready to continue
    public void ContinueAfterTransition()
    {
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
    }
    
    private void DisplayPreviousPlayerMoves()
    {
        if (MoveTracker.Instance != null)
        {
            // Display the previous player's moves
            MoveTracker.Instance.DisplayPreviousPlayerMoves(activeRole);
            
            // Clear the previous player's moves after displaying them
            PlayerRole previousRole = (activeRole == PlayerRole.Robber) ? PlayerRole.Cop : PlayerRole.Robber;
            MoveTracker.Instance.ClearMovesForRole(previousRole);
        }
    }
}