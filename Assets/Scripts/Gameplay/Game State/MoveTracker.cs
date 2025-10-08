using UnityEngine;
using System.Collections.Generic;

public class MoveTracker : MonoBehaviour
{
    [Header("Move History")]
    [SerializeField] private List<PlayerMove> robberMoves = new List<PlayerMove>();
    [SerializeField] private List<PlayerMove> copMoves = new List<PlayerMove>();
    
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Text previousMovesText;
    
    private static MoveTracker instance;
    public static MoveTracker Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MoveTracker>();
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void RecordMove(PlayerRole role, MoveDirection direction)
    {
        PlayerMove move = new PlayerMove(role, direction);
        
        if (role == PlayerRole.Robber)
        {
            robberMoves.Add(move);
        }
        else if (role == PlayerRole.Cop)
        {
            copMoves.Add(move);
        }
        
        Debug.Log($"{role} moved {direction}");
    }
    
    public void ClearMovesForRole(PlayerRole role)
    {
        if (role == PlayerRole.Robber)
        {
            robberMoves.Clear();
        }
        else if (role == PlayerRole.Cop)
        {
            copMoves.Clear();
        }
    }
    
    public List<PlayerMove> GetMovesForRole(PlayerRole role)
    {
        if (role == PlayerRole.Robber)
        {
            return new List<PlayerMove>(robberMoves);
        }
        else if (role == PlayerRole.Cop)
        {
            return new List<PlayerMove>(copMoves);
        }
        return new List<PlayerMove>();
    }
    
    public void DisplayPreviousPlayerMoves(PlayerRole currentPlayerRole)
    {
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager == null || turnManager.previousMovesText == null) 
        {
            return;
        }
        
        PlayerRole previousRole = (currentPlayerRole == PlayerRole.Robber) ? PlayerRole.Cop : PlayerRole.Robber;
        List<PlayerMove> previousMoves = GetMovesForRole(previousRole);
        
        if (previousMoves.Count == 0)
        {
            turnManager.previousMovesText.text = "No previous moves";
            return;
        }
        
        string movesText = $"{previousRole} moves: ";
        for (int i = 0; i < previousMoves.Count; i++)
        {
            movesText += GetArrowForDirection(previousMoves[i].direction);
            if (i < previousMoves.Count - 1)
            {
                movesText += " ";
            }
        }
        
        turnManager.previousMovesText.text = movesText;
    }
    
    private string GetArrowForDirection(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up:
                return "↑";
            case MoveDirection.Down:
                return "↓";
            case MoveDirection.Left:
                return "←";
            case MoveDirection.Right:
                return "→";
            default:
                return "?";
        }
    }
    
    public void ClearAllMoves()
    {
        robberMoves.Clear();
        copMoves.Clear();
        
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null && turnManager.previousMovesText != null)
        {
            turnManager.previousMovesText.text = "";
        }
    }
}

[System.Serializable]
public class PlayerMove
{
    public PlayerRole role;
    public MoveDirection direction;
    
    public PlayerMove(PlayerRole role, MoveDirection direction)
    {
        this.role = role;
        this.direction = direction;
    }
}

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right
}
