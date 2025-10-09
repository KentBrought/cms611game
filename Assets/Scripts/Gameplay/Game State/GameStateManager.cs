using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField]
    private BoardManager boardManager;
    
    [SerializeField]
    private PlayerController robber;
    [SerializeField]
    private PlayerController cop;
    
    [SerializeField]
    private TreasureManager treasureManager;
    
    [Header("Player Settings")]
    [SerializeField]
    private Vector2Int playerStartPosition = new Vector2Int(1, 1);
    
    [Header("Treasure Settings")]
    [SerializeField]
    private int treasureCount = 5;
    
    [Header("Obstacle Settings")]
    [SerializeField]
    private int obstacleCount = 8;
    
    [SerializeField]
    private bool useRandomObstacles = true;
    
    [Header("Game State")]
    private int currentRound = 1;
    
    private void Start()
    {
        InitializeGame();
        InitializeMoveTracker();
    }
    
    private void InitializeGame()
    {
        SetupBoard();
        SetupTreasures();
        SetupPlayers();
        SetupObstacles();
        SetInitialPlayerVisibility();
        
        Debug.Log("Game ready!");
    }
    
    private void SetupBoard()
    {
        if (boardManager == null) return;
        
        if (useRandomObstacles)
        {
            boardManager.InitializeBoardWithoutObstacles();
        }
        else
        {
            boardManager.InitializeBoard();
        }
    }
    
    private void SetupTreasures()
    {
        if (treasureManager != null && boardManager != null)
        {
            treasureManager.Initialize(boardManager, treasureCount);
        }
    }
    
    private void SetupPlayers()
    {
        if (boardManager == null) return;
        
        // Spawn robber first
        if (robber != null)
        {
            Vector2Int robberPos = GetRandomValidPositionAvoiding(Vector2Int.zero);
            robber.SpawnWithRole(boardManager, robberPos, PlayerRole.Robber);
            if (treasureManager != null) treasureManager.RegisterPlayer(robber);
        }
        
        // Spawn cop away from robber
        if (cop != null)
        {
            Vector2Int avoidPos = robber != null ? robber.GetCellPosition() : Vector2Int.zero;
            Vector2Int copPos = GetRandomValidPositionAvoiding(avoidPos);
            cop.SpawnWithRole(boardManager, copPos, PlayerRole.Cop);
            if (treasureManager != null) treasureManager.RegisterPlayer(cop);
        }
    }
    
    private void SetupObstacles()
    {
        if (!useRandomObstacles) return;
        
        InitializeObstacles();
    }
    
    private Vector2Int GetRandomValidPositionAvoiding(Vector2Int avoid)
    {
        if (boardManager == null)
        {
            return playerStartPosition;
        }
        
        for (int i = 0; i < 100; i++)
        {
            Vector2Int pos = new Vector2Int(
                Random.Range(1, boardManager.Width - 1),
                Random.Range(1, boardManager.Height - 1)
            );
            
            if (IsValidPlayerPosition(pos) && pos != avoid)
            {
                return pos;
            }
        }
        
        return playerStartPosition;
    }
    
    private bool IsValidPlayerPosition(Vector2Int position)
    {
        CellData cellData = boardManager.GetCellData(position);
        if (cellData == null || !cellData.Passable)
        {
            return false;
        }
        
        if (treasureManager != null && treasureManager.HasTreasureAt(position))
        {
            return false;
        }
        
        return true;
    }
    
    private void InitializeObstacles()
    {
        if (boardManager == null)
        {
            Debug.LogWarning("BoardManager is null, cannot initialize obstacles");
            return;
        }
        
        List<Vector2Int> occupiedPositions = new List<Vector2Int>();
        
        if (robber != null)
        {
            occupiedPositions.Add(robber.GetCellPosition());
        }
        if (cop != null)
        {
            occupiedPositions.Add(cop.GetCellPosition());
        }
        
        if (treasureManager != null)
        {
            occupiedPositions.AddRange(treasureManager.GetAllTreasurePositions());
        }
        
        boardManager.AddObstaclesAvoidingOverlaps(occupiedPositions, obstacleCount);
        boardManager.RebuildBoardWithObstacles();
        
        Debug.Log($"Added {obstacleCount} obstacles");
    }
    
    public void RestartGame()
    {
        InitializeGame();
    }

    private void SetInitialPlayerVisibility()
    {
        if (cop != null)
        {
            var copRenderer = cop.GetComponent<SpriteRenderer>();
            if (copRenderer != null) copRenderer.enabled = false;
        }
        if (robber != null)
        {
            var robberRenderer = robber.GetComponent<SpriteRenderer>();
            if (robberRenderer != null) robberRenderer.enabled = true;
        }
    }
    
    public void OnPlayerMoved(PlayerController player)
    {
        if (player.GetRole() == PlayerRole.Cop && robber != null)
        {
            // Only check for cop catching robber on the cop's last move of the turn
            TurnManager turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                bool isLastMove = turnManager.GetCurrentMovementSteps() + 1 >= turnManager.GetMaxMovementSteps();
                
                if (isLastMove && robber.GetCellPosition() == player.GetCellPosition())
                {
                    GameSceneManager.Instance.LoadWinScreen("Cop Wins!\nThe robber was caught!");
                    return;
                }
            }
        }
        
        if (player.GetRole() == PlayerRole.Robber && treasureManager != null)
        {
            if (treasureManager.AreAllTreasuresCollected())
            {
                GameSceneManager.Instance.LoadWinScreen("Robber Wins!\nAll treasure collected!");
                return;
            } else if (player.GetCellPosition() == cop.GetCellPosition())
            {
                GameSceneManager.Instance.LoadWinScreen("Cop Wins!\nThe robber ran into the cop!");
                return;
            }
        }
    }
    
    public int GetCurrentRound()
    {
        return currentRound;
    }
    
    public void IncrementRound()
    {
        currentRound++;
        Debug.Log($"Round {currentRound} started");
    }
    
    private void InitializeMoveTracker()
    {
        if (MoveTracker.Instance == null)
        {
            GameObject moveTrackerObj = new GameObject("MoveTracker");
            moveTrackerObj.AddComponent<MoveTracker>();
        }
    }
}
