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
    
    [Header("Indicator Settings")]
    [SerializeField]
    private Sprite indicatorSprite;
    
    private PlayerIndicatorController robberIndicator;
    private PlayerIndicatorController copIndicator;
    
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
        
        // Setup indicators after players are spawned
        SetupIndicators();
    }
    
    private void SetupIndicators()
    {
        if (boardManager == null) return;
        
        // Create robber indicator (starts at cop's position)
        if (robber != null && cop != null)
        {
            GameObject robberIndicatorObj = CreateIndicatorObject("RobberIndicator");
            robberIndicator = robberIndicatorObj.GetComponent<PlayerIndicatorController>();
            robberIndicator.Initialize(boardManager, PlayerRole.Robber, cop.GetCellPosition());
        }
        
        // Create cop indicator (starts at robber's position)
        if (cop != null && robber != null)
        {
            GameObject copIndicatorObj = CreateIndicatorObject("CopIndicator");
            copIndicator = copIndicatorObj.GetComponent<PlayerIndicatorController>();
            copIndicator.Initialize(boardManager, PlayerRole.Cop, robber.GetCellPosition());
        }
        
        // Set initial indicator visibility after indicators are created
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.InitializeIndicatorVisibility();
        }
    }
    
    private GameObject CreateIndicatorObject(string name)
    {
        GameObject indicatorObj = new GameObject(name);
        
        // Set scale to 0.1 on x and y axes
        indicatorObj.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
        
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = indicatorObj.AddComponent<SpriteRenderer>();
        if (indicatorSprite != null)
        {
            spriteRenderer.sprite = indicatorSprite;
        }
        spriteRenderer.sortingOrder = 5; // Above players but below UI
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.8f); // Semi-transparent
        
        // Add PlayerIndicatorController
        PlayerIndicatorController controller = indicatorObj.AddComponent<PlayerIndicatorController>();
        
        return indicatorObj;
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
    
    public PlayerIndicatorController GetIndicatorForRole(PlayerRole role)
    {
        return role == PlayerRole.Robber ? robberIndicator : copIndicator;
    }
    
    public Vector2Int GetIndicatorPosition(PlayerRole role)
    {
        PlayerIndicatorController indicator = GetIndicatorForRole(role);
        return indicator != null ? indicator.GetCellPosition() : Vector2Int.zero;
    }
}
