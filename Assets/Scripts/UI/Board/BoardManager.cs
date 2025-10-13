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
    private int m_ObstacleCount = 8; // Variable value in the future - kira

    private List<Vector2Int> m_Obstacles = new List<Vector2Int>();

    [System.Serializable]
    public class PatternAttributes
    {
        public string Name;
        public bool CanMirror;
        public List<int> CanRotate;           // degrees allowed (e.g., 0,90,180,270)
        public List<Vector2Int> Offsets;      // relative cells (must include (0,0))

        public PatternAttributes(string name, bool mirror, List<int> rotate, List<Vector2Int> offsets)
        {
            Name = name;
            CanMirror = mirror;
            CanRotate = rotate ?? new List<int> { 0 };
            Offsets = offsets ?? new List<Vector2Int> { Vector2Int.zero };
        }
    };

    // Creating all basic pattern types
    // Size 2 obstacle types
    public PatternAttributes line2 = new PatternAttributes(
        "line2",
        false,
        new List<int> { 0, 90 },
        new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0) }
    );

    // Size 3 obstacle types
    public PatternAttributes line3 = new PatternAttributes(
        "line3",
        false,
        new List<int> { 0, 90 },
        new List<Vector2Int> {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0)
        }
    );

    public PatternAttributes corner = new PatternAttributes(
        "corner",
        // You can mirror corners to get all orientations, but rotations already cover it.
        false,
        new List<int> { 0, 90, 180, 270 },
        new List<Vector2Int> {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1)
        }
    );

    // Size 4 obstacle types
    public PatternAttributes line4 = new PatternAttributes(
        "line4",
        false,
        new List<int> { 0, 90 },
        new List<Vector2Int> {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(3, 0)
        }
    );

    public PatternAttributes L = new PatternAttributes(
        "L",
        false,
        new List<int> { 0, 90, 180, 270 },
        new List<Vector2Int> {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2)
        }
    );

    public PatternAttributes box = new PatternAttributes(
        "box",
        false,
        new List<int> { 0 },
        new List<Vector2Int> {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        }
    );

    public PatternAttributes snake = new PatternAttributes(
        "snake",
        true, // mirroring does produce distinct variants here
        new List<int> { 0, 90 },
        new List<Vector2Int> {
            // shape:
            // (0,0) (1,0) (1,1) (2,1)
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1)
        }
    );

    public PatternAttributes T = new PatternAttributes(
        "T",
        false,
        new List<int> { 0, 90, 180, 270 },
        new List<Vector2Int> {
            // shape:
            // (-1,0) (0,0) (1,0) (0,1)
            new Vector2Int(-1, 0),
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1)
        }
    );

    [Header("Pattern Obstacle Settings")]
    [SerializeField] private bool m_UseObstacles = false;
    [SerializeField] private int m_PatternCount = 4; // variable value in future - kira

    [SerializeField] private List<PatternAttributes> m_Patterns = new List<PatternAttributes>
    {
        // seed some defaults? extend in Inspector too?
    };

    private static readonly Vector2Int[] kDirs = new[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

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

        if (m_UseObstacles)
        {
            AddPatternObstaclesAvoidingOverlaps(null, m_PatternCount);
        }

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                // Check if this position is a wall (border) or obstacle
                bool isWall = (x == 0 || y == 0 || x == Width - 1 || y == Height - 1);
                bool isObstacle = m_Obstacles.Contains(new Vector2Int(x, y));

                if (isWall || isObstacle)
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

    private Vector2Int GetRandomEmptyPosition()
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
            // loop until position is NOT invalid (i.e., empty & not border)
        } while (attempts < maxAttempts && IsObstaclePositionInvalid(position));

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

    public void InitializeBoardWithoutObstacles()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        m_BoardData = new CellData[Width, Height];
        m_Obstacles.Clear(); // Clear obstacles for now

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                // Check if this position is a wall (border)
                bool isWall = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;

                if (isWall)
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
            for (int x = 0; x < Width; ++x)
            {
                bool isWall = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
                bool isObstacle = m_Obstacles.Contains(new Vector2Int(x, y));

                if (isWall || isObstacle)
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