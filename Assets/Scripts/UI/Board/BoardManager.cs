using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CellData
{
    public bool Passable;
}
public class BoardManager : MonoBehaviour
{
    
    private Tilemap m_Tilemap;
    private CellData[,] m_BoardData;

    public int Width;
    public int Height;
    public Tile RoadTile;
    public Tile RoofTile;
    private Grid m_Grid;

    [Header("Obstacle Settings")]
    [SerializeField]
    private int m_ObstacleCount = 8;
    
    [SerializeField]
    private bool m_UseRandomObstacles = true;
    
    private List<Vector2Int> m_Obstacles = new List<Vector2Int>();


    // Start is called before the first frame update
    public PlayerController Player;
    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

// Start is called before the first frame update
    void Start()
    {
        // Board initialization is now handled by GameStateManager
        // This ensures proper order: board -> treasures -> player -> obstacles
    }
    
    public void InitializeBoard()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        
        m_BoardData = new CellData[Width, Height];
        
        if (m_UseRandomObstacles)
        {
            GenerateRandomObstacles();
        }

        for (int y = 0; y < Height; ++y)
        {
            for(int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                // Check if this position is a wall (border) or obstacle
                bool isWall = (x == 0 || y == 0 || x == Width - 1 || y == Height - 1);
                bool isObstacle = m_Obstacles.Contains(new Vector2Int(x, y));
                
                if(isWall || isObstacle)
                {
                    tile = RoofTile;
                    m_BoardData[x, y].Passable = false;
                    
                    // Apply random rotation to rooftiles
                    Vector3Int position = new Vector3Int(x, y, 0);
                    m_Tilemap.SetTile(position, tile);
                    
                    Matrix4x4 transform = Matrix4x4.TRS(Vector3.zero, 
                        Quaternion.Euler(0, 0, Random.Range(0, 4) * 90), 
                        Vector3.one);
                    m_Tilemap.SetTransformMatrix(position, transform);
                }
                else
                {
                    tile = RoadTile;
                    m_BoardData[x, y].Passable = true;
                    m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
    
    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width
            || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }
    
    private void GenerateRandomObstacles()
    {
        m_Obstacles.Clear();
        
        int attempts = 0;
        const int maxAttempts = 1000;
        
        for (int i = 0; i < m_ObstacleCount && attempts < maxAttempts; i++)
        {
            Vector2Int position = GetRandomObstaclePosition();
            if (position != Vector2Int.zero)
            {
                m_Obstacles.Add(position);
            }
            attempts++;
        }
        
        Debug.Log($"Generated {m_Obstacles.Count} random obstacles");
    }
    
    private Vector2Int GetRandomObstaclePosition()
    {
        Vector2Int position;
        int attempts = 0;
        const int maxAttempts = 100;
        
        do
        {
            position = new Vector2Int(
                Random.Range(1, Width - 1),
                Random.Range(1, Height - 1)
            );
            attempts++;
        }
        while (attempts < maxAttempts && IsObstaclePositionInvalid(position));
        
        return attempts >= maxAttempts ? Vector2Int.zero : position;
    }
    
    private bool IsObstaclePositionInvalid(Vector2Int position)
    {
        // Check if position is already an obstacle
        if (m_Obstacles.Contains(position))
        {
            return true;
        }
        
        // Check if position is on the border (walls)
        if (position.x == 0 || position.y == 0 || position.x == Width - 1 || position.y == Height - 1)
        {
            return true;
        }
        
        return false;
    }
    
    // Method to set obstacles from external sources (for collision avoidance)
    public void SetObstacles(List<Vector2Int> obstacles)
    {
        m_Obstacles = new List<Vector2Int>(obstacles);
    }
    
    // Method to add obstacles while avoiding overlaps
    public void AddObstaclesAvoidingOverlaps(List<Vector2Int> positionsToAvoid, int obstacleCount = -1)
    {
        m_Obstacles.Clear();
        
        int targetCount = obstacleCount > 0 ? obstacleCount : m_ObstacleCount;
        int attempts = 0;
        const int maxAttempts = 1000;
        
        for (int i = 0; i < targetCount && attempts < maxAttempts; i++)
        {
            Vector2Int position = GetRandomObstaclePositionAvoidingOverlaps(positionsToAvoid);
            if (position != Vector2Int.zero)
            {
                m_Obstacles.Add(position);
            }
            attempts++;
        }
        
        Debug.Log($"Generated {m_Obstacles.Count} random obstacles avoiding overlaps");
    }
    
    private Vector2Int GetRandomObstaclePositionAvoidingOverlaps(List<Vector2Int> positionsToAvoid)
    {
        Vector2Int position;
        int attempts = 0;
        const int maxAttempts = 100;
        
        do
        {
            position = new Vector2Int(
                Random.Range(1, Width - 1),
                Random.Range(1, Height - 1)
            );
            attempts++;
        }
        while (attempts < maxAttempts && 
               (IsObstaclePositionInvalid(position) || positionsToAvoid.Contains(position)));
        
        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"Could not find valid obstacle position after {maxAttempts} attempts");
        }
        
        return attempts >= maxAttempts ? Vector2Int.zero : position;
    }
    
    public void InitializeBoardWithoutObstacles()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        
        m_BoardData = new CellData[Width, Height];
        m_Obstacles.Clear(); // Clear obstacles for now

        for (int y = 0; y < Height; ++y)
        {
            for(int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                // Check if this position is a wall (border)
                bool isWall = (x == 0 || y == 0 || x == Width - 1 || y == Height - 1);
                
                if(isWall)
                {
                    tile = RoofTile;
                    m_BoardData[x, y].Passable = false;
                    
                    // Apply random rotation to rooftiles
                    Vector3Int position = new Vector3Int(x, y, 0);
                    m_Tilemap.SetTile(position, tile);
                    
                    Matrix4x4 transform = Matrix4x4.TRS(Vector3.zero, 
                        Quaternion.Euler(0, 0, Random.Range(0, 4) * 90), 
                        Vector3.one);
                    m_Tilemap.SetTransformMatrix(position, transform);
                }
                else
                {
                    tile = RoadTile;
                    m_BoardData[x, y].Passable = true;
                    m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
    
    public void RebuildBoardWithObstacles()
    {
        if (m_Tilemap == null || m_BoardData == null)
        {
            Debug.LogWarning("Board not properly initialized");
            return;
        }
        
        // Update board data with obstacles
        for (int y = 0; y < Height; ++y)
        {
            for(int x = 0; x < Width; ++x)
            {
                bool isWall = (x == 0 || y == 0 || x == Width - 1 || y == Height - 1);
                bool isObstacle = m_Obstacles.Contains(new Vector2Int(x, y));
                
                if(isWall || isObstacle)
                {
                    Tile tile = RoofTile;
                    m_BoardData[x, y].Passable = false;
                    
                    // Apply random rotation to rooftiles
                    Vector3Int position = new Vector3Int(x, y, 0);
                    m_Tilemap.SetTile(position, tile);
                    
                    Matrix4x4 transform = Matrix4x4.TRS(Vector3.zero, 
                        Quaternion.Euler(0, 0, Random.Range(0, 4) * 90), 
                        Vector3.one);
                    m_Tilemap.SetTransformMatrix(position, transform);
                }
                else
                {
                    Tile tile = RoadTile;
                    m_BoardData[x, y].Passable = true;
                    m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Initialize Board for Debug")]
    private void InitializeBoardForDebug()
    {
        InitializeBoardWithoutObstacles();
        
        UnityEditor.SceneView.RepaintAll();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("Initialize Board with Obstacles for Debug")]
    private void InitializeBoardWithObstaclesForDebug()
    {
        InitializeBoard();
        
        UnityEditor.SceneView.RepaintAll();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    [ContextMenu("Clear Board")]
    private void ClearBoard()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        if (m_Tilemap != null)
        {
            m_Tilemap.ClearAllTiles();
            UnityEditor.SceneView.RepaintAll();
        }
        else
        {
            Debug.LogError("Tilemap component not found.");
        }
    }
#endif
}