using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public Text turnCounterText; 
    public Button nextTurnButton; 
    private int turnCount = 1;
    private bool hasMovedThisTurn = false;

    void Start()
    {
        UpdateTurnCounter();
        nextTurnButton.onClick.AddListener(NextTurn);
    }

    public bool CanMoveThisTurn()
    {
        return !hasMovedThisTurn;
    }

    public void CharacterMoved()
    {
        if (!hasMovedThisTurn)
        {
            hasMovedThisTurn = true;
            Debug.Log("Character moved this turn.");
        }
        else
        {
            Debug.Log("Character can only move once per turn.");
        }
    }

    private void NextTurn()
    {
        turnCount++;
        hasMovedThisTurn = false;
        UpdateTurnCounter();
        Debug.Log("Next turn started.");
    }

    private void UpdateTurnCounter()
    {
        turnCounterText.text = "Turn: " + turnCount;
    }
}