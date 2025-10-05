using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public Text turnCounterText; 
    public Text coinCounterText;  // New UI text for coin display
    public Text movementStepsText;  // New UI text for movement steps display
    public Button nextTurnButton; 
    private int turnCount = 1;
    private int currentMovementSteps = 0;
    private int maxMovementSteps = 0;

    void Start()
    {
        UpdateTurnCounter();
        UpdateCoinDisplay(0);  // Initialize coin display
        GenerateNewMovementSteps();  // Generate initial movement steps
        nextTurnButton.onClick.AddListener(NextTurn);
    }

    public bool CanMoveThisTurn()
    {
        return currentMovementSteps < maxMovementSteps;
    }

    public void CharacterMoved()
    {
        if (currentMovementSteps < maxMovementSteps)
        {
            currentMovementSteps++;
            UpdateMovementStepsDisplay();
            Debug.Log($"Character moved this turn. Steps used: {currentMovementSteps}/{maxMovementSteps}");
        }
        else
        {
            Debug.Log("Character has used all movement steps this turn.");
        }
    }

    private void NextTurn()
    {
        turnCount++;
        GenerateNewMovementSteps();
        UpdateTurnCounter();
        Debug.Log($"Next turn started. Movement steps available: {maxMovementSteps}");
    }

    private void UpdateTurnCounter()
    {
        turnCounterText.text = "Turn: " + turnCount;
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
    }
    
    private void UpdateMovementStepsDisplay()
    {
        if (movementStepsText != null)
        {
            movementStepsText.text = $"Steps: {currentMovementSteps}/{maxMovementSteps}";
        }
    }
}