using UnityEngine;

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
    
    [Header("Future: Obstacle Settings")]
    [SerializeField]
    private int m_ObstacleCount = 0; // For future obstacle randomization
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // Initialize board first
        if (m_BoardManager != null)
        {
            m_BoardManager.InitializeBoard();
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
        
        // Future: Initialize obstacles
        // InitializeObstacles();
        
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
    
    // Future method for obstacle initialization
    private void InitializeObstacles()
    {
        // This will be implemented when you add random obstacle generation
        Debug.Log("Obstacle initialization - to be implemented");
    }
    
    // Public method to reinitialize game (useful for restart functionality)
    public void RestartGame()
    {
        InitializeGame();
    }
}
