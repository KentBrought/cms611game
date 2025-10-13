using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq; // (Distinct/Any helpers)
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
    private int m_ObstacleCount = 4; // Variable value in the future - kira

    private List<List<Vector2Int>> m_Obstacles = new List<List<Vector2Int>>(); // check syntax

    [System.Serializable]
    public class PatternAttributes
    {
        public string Name;
        public bool CanMirror;
        public List<int> CanRotate;           // degrees allowed (e.g., 0,90,180,270)
        public List<Vector2Int> Offsets;      // relative cells (should include (0,0))

        public PatternAttributes(string name, bool mirror, List<int> rotate, List<Vector2Int> offsets)
        {
            Name = name;
            CanMirror = mirror;
            CanRotate = rotate ?? new List<int> { 0 };
            Offsets = offsets ?? new List<Vector2Int> { Vector2Int.zero };
        }
    };

    // Basic pattern types
    public PatternAttributes line2 = new PatternAttributes(
        "line2",
        false,
        new List<int> { 0, 90 },
        new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 0) }
    );

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
        false,
        new List<int> { 0, 90, 180, 270 },
        new List<Vector2Int> {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1)
        }
    );

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
        true, // mirroring produces distinct variants here
        new List<int> { 0, 90 },
        new List<Vector2Int> {
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
            new Vector2Int(-1, 0),
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1)
        }
    );

    [Header("Pattern Obstacle Settings")]
    [SerializeField] private bool m_UseObstacles = true;
    [SerializeField] private List<PatternAttributes> m_Patterns = new List<PatternAttributes>
    {
        // You can populate in Inspector; we also auto-seed if empty.
    };

    // ensure we keep board connected when placing patterns
    [SerializeField] private bool m_RequireFullConnectivity = true;

    private static readonly Vector2Int[] kDirs = new[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public PlayerController Player;

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    void Start()
    {
        // Board initialization is now handled by GameStateManager
        // Order: board -> treasures -> player -> obstacles
    }

    public void InitializeBoard()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        m_BoardData = new CellData[Width, Height];

        if (m_UseObstacles)
        {
            AddPatternObstaclesAvoidingOverlaps(null, m_ObstacleCount); // changed
        }

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                bool isWall = (x == 0 || y == 0 || x == Width - 1 || y == Height - 1);
                bool isObstacle = false;
                foreach (var pattern in m_Obstacles)
                {
                    if (pattern.Contains(new Vector2Int(x, y))) isObstacle = true;
                }

                if (isWall || isObstacle)
                {
                    tile = RoofTile;
                    m_BoardData[x, y].Passable = false;

                    Vector3Int position = new Vector3Int(x, y, 0);
                    m_Tilemap.SetTile(position, tile);

                    Matrix4x4 transform = Matrix4x4.TRS(
                        Vector3.zero,
                        Quaternion.Euler(0, 0, Random.Range(0, 4) * 90),
                        Vector3.one
                    );
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
        } while (attempts < maxAttempts && IsObstaclePositionInvalid(position));

        return attempts >= maxAttempts ? Vector2Int.zero : position;
    }

    private bool IsObstaclePositionInvalid(Vector2Int position)
    {
        foreach (var pattern in m_Obstacles)
        {
            if (pattern.Contains(coord)) return true;
        };

        if (position.x == 0 || position.y == 0 || position.x == Width - 1 || position.y == Height - 1)
            return true;

        return false;
    }

    public void SetObstacles(List<List<Vector2Int>> obstacles)
    {
        m_Obstacles = new List<List<Vector2Int>>(obstacles);
    }

    public void InitializeBoardWithoutObstacles()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();

        m_BoardData = new CellData[Width, Height];
        m_Obstacles.Clear();

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                bool isWall = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;

                if (isWall)
                {
                    tile = RoofTile;
                    m_BoardData[x, y].Passable = false;

                    Vector3Int position = new Vector3Int(x, y, 0);
                    m_Tilemap.SetTile(position, tile);

                    Matrix4x4 transform = Matrix4x4.TRS(
                        Vector3.zero,
                        Quaternion.Euler(0, 0, Random.Range(0, 4) * 90),
                        Vector3.one
                    );
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

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                bool isWall = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
                bool isObstacle = false; 
                foreach (var pattern in m_Obstacles)
                {
                    if (pattern.Contains(new Vector2Int(x, y))) return true;
                }

                if (isWall || isObstacle)
                {
                    Tile tile = RoofTile;
                    m_BoardData[x, y].Passable = false;

                    Vector3Int position = new Vector3Int(x, y, 0);
                    m_Tilemap.SetTile(position, tile);

                    Matrix4x4 transform = Matrix4x4.TRS(
                        Vector3.zero,
                        Quaternion.Euler(0, 0, Random.Range(0, 4) * 90),
                        Vector3.one
                    );
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

    // ===================== NEW + COMPAT APIs ==========================

    // /// <summary>
    // /// COMPAT: legacy signature kept alive for callers (e.g., GameStateManager).
    // /// If you pass positionsToAvoid, they will be respected.
    // /// If pattern logic is desired, we route to pattern placement; otherwise we place single tiles.
    // /// </summary>
    // public void AddObstaclesAvoidingOverlaps(List<Vector2Int> positionsToAvoid, int obstacleCount = -1) // COMPAT
    // {
    //     int count = (obstacleCount > 0) ? obstacleCount : m_ObstacleCount;

    //     // If you have patterns configured, prefer pattern placement with 'count' patterns.
    //     if (m_Patterns != null && m_Patterns.Count > 0)
    //     {
    //         AddPatternObstaclesAvoidingOverlaps(positionsToAvoid, count); // CHANGED: reuse pattern path
    //         return;
    //     }

    //     // Fallback: single-cell scatter while avoiding walls/overlaps/reserved
    //     m_Obstacles.Clear();
    //     int attempts = 0;
    //     const int maxAttempts = 2000;

    //     while (m_Obstacles.Count < count && attempts < maxAttempts)
    //     {
    //         attempts++;
    //         var pos = new Vector2Int(Random.Range(1, Width - 1), Random.Range(1, Height - 1));
    //         if (IsObstaclePositionInvalid(pos)) continue;
    //         if (positionsToAvoid != null && positionsToAvoid.Contains(pos)) continue;

    //         // tentatively add and ensure connectivity if required
    //         m_Obstacles.Add(pos);
    //         if (m_RequireFullConnectivity && !IsBoardFullyConnected())
    //         {
    //             m_Obstacles.Remove(pos);
    //             continue;
    //         }
    //     }
    // }

    /// <summary>
    /// Place multi-cell obstacle patterns (lines, corners, etc.) randomly,
    /// avoiding reserved cells and ensuring the remaining free cells stay connected.
    /// </summary>
    public void AddPatternObstaclesAvoidingOverlaps(List<Vector2Int> positionsToAvoid, int obstacleCount = -1)
    {
        // Auto-seed patterns if empty so it "just works"
        if (m_Patterns == null || m_Patterns.Count == 0)
        {
            m_Patterns = new List<PatternAttributes> { line2, line3, corner, line4, L, box, snake, T };
        }

        m_Obstacles.Clear();

        int target = (obstacleCount > 0) ? obstacleCount : m_ObstacleCount;
        int attempts = 0;
        const int maxAttempts = 4000;

        while (m_Obstacles.Count < target && attempts < maxAttempts)
        {
            attempts++;

            var pattern = m_Patterns[Random.Range(0, m_Patterns.Count)];
            var variants = GeneratePatternVariants(pattern);
            Shuffle(variants);

            bool placed = false;

            foreach (var offsets in variants)
            {
                // try random origins (a few times) per variant
                for (int tries = 0; tries < 20 && !placed; tries++)
                {
                    Vector2Int origin = new Vector2Int(Random.Range(1, Width - 1), Random.Range(1, Height - 1));
                    if (!CanPlacePattern(offsets, origin, positionsToAvoid)) continue;

                    var added = ApplyPatternCells(offsets, origin);

                    if (!m_RequireFullConnectivity || IsBoardFullyConnected())
                    {
                        placed = true;
                        break;
                    }
                    // rollback
                    m_Obstacles.Remove(added);
                }
                if (placed) break;
            }
            // if not placed, loop continues until attempts cap
        }
    }

    // Create transformed variants based on rotation degrees and optional mirror
    private List<Vector2Int[]> GeneratePatternVariants(PatternAttributes pattern)
    {
        var variants = new List<Vector2Int[]>();

        var degrees = (pattern.CanRotate != null && pattern.CanRotate.Count > 0)
            ? pattern.CanRotate.Distinct().Select(d => ((d % 360) + 360) % 360).ToList()
            : new List<int> { 0 };

        foreach (var deg in degrees)
        {
            var baseRot = RotateOffsets(pattern.Offsets, deg);
            variants.Add(baseRot);
            if (pattern.CanMirror)
            {
                variants.Add(Mirror(baseRot));
            }
        }
        return variants;
    }

    private static Vector2Int[] RotateOffsets(List<Vector2Int> src, int deg)
    {
        Vector2Int Rot(Vector2Int o, int d)
        {
            switch (d % 360)
            {
                case 90:  return new Vector2Int(-o.y,  o.x);
                case 180: return new Vector2Int(-o.x, -o.y);
                case 270: return new Vector2Int( o.y, -o.x);
                default:  return o;
            }
        }
        var dst = new Vector2Int[src.Count];
        for (int i = 0; i < src.Count; i++) dst[i] = Rot(src[i], deg);
        return dst;
    }

    private static Vector2Int[] Mirror(Vector2Int[] src)
    {
        var dst = new Vector2Int[src.Length];
        for (int i = 0; i < src.Length; i++) dst[i] = new Vector2Int(-src[i].x, src[i].y);
        return dst;
    }

    private bool CanPlace(Vector2Int coord, List<Vector2Int> avoid)
    {
        if (coord.x <= 0 || coord.y <= 0 || coord.x >= Width - 1 || coord.y >= Height - 1)
            return false;

        foreach (var pattern in m_Obstacles)
        {
            if (pattern.Contains(coord)) return false;
        }
        if (avoid != null && avoid.Contains(coord)) return false;
        return true;
    }

    private bool CanPlacePattern(Vector2Int[] offsets, Vector2Int origin, List<Vector2Int> avoid)
    {
        foreach (var off in offsets)
        {
            var coord = origin + off;
            if (!CanPlace(coord, avoid)) return false;
        }
        return true;
    }

    private List<Vector2Int> ApplyPatternCells(Vector2Int[] offsets, Vector2Int origin)
    {
        var added = new List<Vector2Int>(offsets.Length);
        foreach (var off in offsets)
        {
            var p = origin + off;
            added.Add(p);
        }
        m_Obstacles.Add(added);

        return added;
    }

    private bool IsBoardFullyConnected() // make sure obstacles haven't cut off board
    {
        int totalPassable = 0;
        Vector2Int? seed = null;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                bool isBorder   = (x == 0 || y == 0 || x == Width - 1 || y == Height - 1);
                bool isObstacle = false;
                foreach (var pattern in m_Obstacles)
                {
                    if (pattern.Contains(new Vector2Int(x, y))) isObstacle = true;
                }
                bool passable   = !isBorder && !isObstacle;
                if (passable)
                {
                    totalPassable++;
                    if (!seed.HasValue) seed = new Vector2Int(x, y);
                }
            }
        }

        if (totalPassable == 0) return true;

        var visited = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();
        q.Enqueue(seed.Value);
        visited.Add(seed.Value);

        while (q.Count > 0)
        {
            var c = q.Dequeue();
            foreach (var d in kDirs)
            {
                var n = c + d;
                if (n.x <= 0 || n.y <= 0 || n.x >= Width - 1 || n.y >= Height - 1) continue;
                if (visited.Contains(n)) continue;
                foreach (var pattern in m_Obstacles)
                {
                    if (pattern.Contains(n)) continue; // blocked
                }
                visited.Add(n);
                q.Enqueue(n);
            }
        }

        return visited.Count == totalPassable;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
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
