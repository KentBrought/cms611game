using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField]
    private BoardManager m_BoardManager;
    
    [SerializeField]
    private PlayerController m_Robber;
    [SerializeField]
    private PlayerController m_Cop;
    
    [SerializeField]
    private TreasureManager m_TreasureManager;
    
    [Header("Player Settings")]
    [SerializeField]
    private Vector2Int m_PlayerStartPosition = new Vector2Int(1, 1); // Fallback if random fails
    
    [Header("Treasure Settings")]
    [SerializeField]
    private int m_TreasureCount = 5;
    
    [Header("Obstacle Settings")]
    [SerializeField]
    private int m_ObstacleCount = 8;
    
    [SerializeField]
    private bool m_UseRandomObstacles = true;
    
    [Header("Game State")]
    private int m_CurrentRound = 1;
    
    private void Start()
    {
        InitializeGame();
        InitializeMoveTracker();
    }
    
    private void InitializeGame()
    {
        // Initialize board first
        if (m_BoardManager != null)
        {
            if (m_UseRandomObstacles)
            {
                // Initialize board without obstacles first
                m_BoardManager.InitializeBoardWithoutObstacles();
            }
            else
            {
                m_BoardManager.InitializeBoard();
            }
        }
        
        // Initialize treasures first (so we can avoid spawning player on treasure)
        if (m_TreasureManager != null && m_BoardManager != null)
        {
            m_TreasureManager.Initialize(m_BoardManager, m_TreasureCount);
        }
        
        // Initialize both players: Robber and Cop (pass-and-play)
        if (m_BoardManager != null)
        {
            // Robber
            if (m_Robber != null)
            {
                Vector2Int robberPos = GetRandomValidPositionAvoiding(Vector2Int.zero);
                m_Robber.SpawnWithRole(m_BoardManager, robberPos, PlayerRole.Robber);
                if (m_TreasureManager != null) m_TreasureManager.RegisterPlayer(m_Robber);
            }
            // Cop avoid robber start
            if (m_Cop != null)
            {
                Vector2Int avoid = m_Robber != null ? m_Robber.GetCellPosition() : Vector2Int.zero;
                Vector2Int copPos = GetRandomValidPositionAvoiding(avoid);
                m_Cop.SpawnWithRole(m_BoardManager, copPos, PlayerRole.Cop);
                if (m_TreasureManager != null) m_TreasureManager.RegisterPlayer(m_Cop);
            }
        }
        
        // Initialize obstacles with collision avoidance
        if (m_UseRandomObstacles)
        {
            InitializeObstacles();
        }
        
        // Set initial visibility: only show the active player (robber starts)
        SetInitialPlayerVisibility();
        
        Debug.Log("Game initialization complete!");
    }
    
    private Vector2Int GetRandomValidPositionAvoiding(Vector2Int avoid)
    {
        if (m_BoardManager == null)
        {
            return m_PlayerStartPosition;
        }
        Vector2Int position;
        int attempts = 0;
        const int maxAttempts = 100;
        do
        {
            position = new Vector2Int(
                Random.Range(1, m_BoardManager.Width - 1),
                Random.Range(1, m_BoardManager.Height - 1)
            );
            attempts++;
        }
        while (attempts < maxAttempts && (
            !IsValidPlayerPosition(position) || (avoid != Vector2Int.zero && position == avoid)));
        return attempts >= maxAttempts ? m_PlayerStartPosition : position;
    }
    
    private bool IsValidPlayerPosition(Vector2Int position)
    {
        // Check if position is passable
        CellData cellData = m_BoardManager.GetCellData(position);
        if (cellData == null || !cellData.Passable)
        {
            return false;
        }
        
        // Check if position is not occupied by treasure
        if (m_TreasureManager != null && m_TreasureManager.HasTreasureAt(position))
        {
            return false;
        }
        
        return true;
    }
    
    private void InitializeObstacles()
    {
        if (m_BoardManager == null)
        {
            Debug.LogWarning("BoardManager is null, cannot initialize obstacles");
            return;
        }
        
        // Collect positions to avoid (player and treasures)
        List<Vector2Int> positionsToAvoid = new List<Vector2Int>();
        
        // Add players' positions (robber and cop) if available
        if (m_Robber != null)
        {
            positionsToAvoid.Add(m_Robber.GetCellPosition());
        }
        if (m_Cop != null)
        {
            positionsToAvoid.Add(m_Cop.GetCellPosition());
        }
        
        // Add treasure positions
        if (m_TreasureManager != null)
        {
            List<Vector2Int> treasurePositions = m_TreasureManager.GetAllTreasurePositions();
            positionsToAvoid.AddRange(treasurePositions);
        }
        
        // Generate obstacles avoiding overlaps
        m_BoardManager.AddObstaclesAvoidingOverlaps(positionsToAvoid, m_ObstacleCount);
        
        // Rebuild the board with the new obstacles
        m_BoardManager.RebuildBoardWithObstacles();
        
        Debug.Log($"Initialized {m_ObstacleCount} obstacles avoiding {positionsToAvoid.Count} occupied positions");
    }
    
    // Public method to reinitialize game (useful for restart functionality)
    public void RestartGame()
    {
        InitializeGame();
    }

    private void SetInitialPlayerVisibility()
    {
        // Hide cop initially, show robber (robber starts first)
        if (m_Cop != null)
        {
            var copRenderer = m_Cop.GetComponent<SpriteRenderer>();
            if (copRenderer != null) copRenderer.enabled = false;
        }
        if (m_Robber != null)
        {
            var robberRenderer = m_Robber.GetComponent<SpriteRenderer>();
            if (robberRenderer != null) robberRenderer.enabled = true;
        }
    }
    
    public void OnPlayerMoved(PlayerController player)
    {
        // Check for cop catching robber
        if (player.GetRole() == PlayerRole.Cop && m_Robber != null)
        {
            if (m_Robber.GetCellPosition() == player.GetCellPosition())
            {
                Debug.Log("Cop wins: robber caught!");
                GameSceneManager.Instance.LoadWinScreen("Cop");
                return;
            }
        }
        
        // Check for robber collecting all treasure
        if (player.GetRole() == PlayerRole.Robber && m_TreasureManager != null)
        {
            if (m_TreasureManager.AreAllTreasuresCollected())
            {
                Debug.Log("Robber wins: all treasure collected!");
                GameSceneManager.Instance.LoadWinScreen("Robber");
                return;
            }
        }
    }
    
    public int GetCurrentRound()
    {
        return m_CurrentRound;
    }
    
    public void IncrementRound()
    {
        m_CurrentRound++;
        Debug.Log($"Round {m_CurrentRound} started");
    }
    
    private void InitializeMoveTracker()
    {
        // Ensure MoveTracker exists in the scene
        if (MoveTracker.Instance == null)
        {
            GameObject moveTrackerObj = new GameObject("MoveTracker");
            moveTrackerObj.AddComponent<MoveTracker>();
            Debug.Log("Created MoveTracker instance");
        }
    }
}
