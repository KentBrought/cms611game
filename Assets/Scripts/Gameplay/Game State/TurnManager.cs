using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public Text turnCounterText; 
    public Text coinCounterText;
    public Text movementStepsText;
    public Text previousMovesText;
    public Button nextTurnButton; 
    private int turnCount = 1;
    private int currentMovementSteps = 0;
    private int maxMovementSteps = 0;
    private PlayerRole activeRole = PlayerRole.Robber;

    void Start()
    {
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
        
        GameStateManager gameState = FindFirstObjectByType<GameStateManager>();
        if (gameState != null)
        {
            gameState.IncrementRound();
        }
        
        GameSceneManager.Instance.LoadTransitionScreen();
        if (nextTurnButton != null) nextTurnButton.interactable = false;
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
            MoveTracker.Instance.DisplayPreviousPlayerMoves(activeRole);
            
            PlayerRole previousRole = (activeRole == PlayerRole.Robber) ? PlayerRole.Cop : PlayerRole.Robber;
            MoveTracker.Instance.ClearMovesForRole(previousRole);
        }
    }
}