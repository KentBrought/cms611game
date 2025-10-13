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
        if (!boardManager) return;
        
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
        if (treasureManager && boardManager)
        {
            treasureManager.Initialize(boardManager, treasureCount);
        }
    }
    
    private void SetupPlayers()
    {
        if (!boardManager) return;
        
        // Spawn robber first
        if (robber)
        {
            Vector2Int robberPos = GetRandomValidPositionAvoiding(Vector2Int.zero);
            robber.SpawnWithRole(boardManager, robberPos, PlayerRole.Robber);
            if (treasureManager) treasureManager.RegisterPlayer(robber);
        }
        
        // Spawn cop away from robber
        if (cop)
        {
            Vector2Int avoidPos = robber ? robber.GetCellPosition() : Vector2Int.zero;
            Vector2Int copPos = GetRandomValidPositionAvoiding(avoidPos);
            cop.SpawnWithRole(boardManager, copPos, PlayerRole.Cop);
            if (treasureManager) treasureManager.RegisterPlayer(cop);
        }
        
        // Setup indicators after players are spawned
        SetupIndicators();
    }
    
    private void SetupIndicators()
    {
        if (!boardManager) return;
        
        // Create robber indicator (starts at cop's position)
        if (robber && cop)
        {
            GameObject robberIndicatorObj = CreateIndicatorObject("RobberIndicator");
            robberIndicator = robberIndicatorObj.GetComponent<PlayerIndicatorController>();
            robberIndicator.Initialize(boardManager, PlayerRole.Robber, cop.GetCellPosition());
        }
        
        // Create cop indicator (starts at robber's position)
        if (cop && robber)
        {
            GameObject copIndicatorObj = CreateIndicatorObject("CopIndicator");
            copIndicator = copIndicatorObj.GetComponent<PlayerIndicatorController>();
            copIndicator.Initialize(boardManager, PlayerRole.Cop, robber.GetCellPosition());
        }
        
        // Set initial indicator visibility after indicators are created
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager)
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
        if (indicatorSprite)
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
        if (!boardManager)
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
        if (!cellData || !cellData.Passable)
        {
            return false;
        }
        
        if (treasureManager && treasureManager.HasTreasureAt(position))
        {
            return false;
        }
        
        return true;
    }
    
    private void InitializeObstacles()
    {
        if (!boardManager)
        {
            Debug.LogWarning("BoardManager is null, cannot initialize obstacles");
            return;
        }
        
        List<Vector2Int> occupiedPositions = new List<Vector2Int>();
        
        if (robber)
        {
            occupiedPositions.Add(robber.GetCellPosition());
        }
        if (cop)
        {
            occupiedPositions.Add(cop.GetCellPosition());
        }
        
        if (treasureManager)
        {
            occupiedPositions.AddRange(treasureManager.GetAllTreasurePositions());
        }
        
        boardManager.AddPatternObstaclesAvoidingOverlaps(occupiedPositions, obstacleCount);
        boardManager.RebuildBoardWithObstacles();
        
        Debug.Log($"Added {obstacleCount} obstacles");
    }
    
    public void RestartGame()
    {
        InitializeGame();
    }

    private void SetInitialPlayerVisibility()
    {
        if (cop)
        {
            var copRenderer = cop.GetComponent<SpriteRenderer>();
            if (copRenderer) copRenderer.enabled = false;
        }
        if (robber)
        {
            var robberRenderer = robber.GetComponent<SpriteRenderer>();
            if (robberRenderer) robberRenderer.enabled = true;
        }
    }
    
    public void OnPlayerMoved(PlayerController player)
    {
        if (player.GetRole() == PlayerRole.Cop && robber)
        {
            // Only check for cop catching robber on the cop's last move of the turn
            TurnManager turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager)
            {
                bool isLastMove = turnManager.GetCurrentMovementSteps() + 1 >= turnManager.GetMaxMovementSteps();
                
                if (isLastMove && robber.GetCellPosition() == player.GetCellPosition())
                {
                    GameSceneManager.Instance.LoadWinScreen("Cop Wins!\nThe robber was caught!");
                    return;
                }
            }
        }
        
        if (player.GetRole() == PlayerRole.Robber && treasureManager)
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
        if (!MoveTracker.Instance)
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
        return indicator ? indicator.GetCellPosition() : Vector2Int.zero;
    }
}
