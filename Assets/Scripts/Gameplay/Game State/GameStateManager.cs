using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField]
    private BoardManager m_BoardManager;
    
    [SerializeField]
    private PlayerController m_Player;
    
    [SerializeField]
    private TreasureManager m_TreasureManager;
    
    [Header("Player Settings")]
    [SerializeField]
    private bool m_UseRandomStartPosition = true;
    
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
    
    private void Start()
    {
        InitializeGame();
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
        
        // Initialize player
        if (m_Player != null && m_BoardManager != null)
        {
            Vector2Int playerPosition = m_UseRandomStartPosition ? GetRandomPlayerPosition() : m_PlayerStartPosition;
            m_Player.Spawn(m_BoardManager, playerPosition);
            
            // Register player with treasure manager after spawning
            if (m_TreasureManager != null)
            {
                m_TreasureManager.RegisterPlayer(m_Player);
            }
        }
        
        // Initialize obstacles with collision avoidance
        if (m_UseRandomObstacles)
        {
            InitializeObstacles();
        }
        
        Debug.Log("Game initialization complete!");
    }
    
    private Vector2Int GetRandomPlayerPosition()
    {
        if (m_BoardManager == null)
        {
            Debug.LogWarning("BoardManager is null, using fallback position");
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
        while (attempts < maxAttempts && !IsValidPlayerPosition(position));
        
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("Could not find valid random position, using fallback");
            return m_PlayerStartPosition;
        }
        
        Debug.Log($"Player spawned at random position: {position}");
        return position;
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
        
        // Add player position
        if (m_Player != null)
        {
            Vector2Int playerPos = m_Player.GetCellPosition();
            positionsToAvoid.Add(playerPos);
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
}
