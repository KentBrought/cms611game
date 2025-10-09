using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

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
    public Tile[] GroundTiles;
    public Tile[] WallTiles;
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
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
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
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
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
                    Tile tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                    m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
                else
                {
                    Tile tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    // ===================== Pattern-based API ==========================

    /// <summary>
    /// Place multi-cell obstacle patterns (lines, corners, etc.) randomly,
    /// avoiding reserved cells and ensuring that the remaining free
    /// cells form one connected component.
    /// </summary>
    public void AddPatternObstaclesAvoidingOverlaps(List<Vector2Int> positionsToAvoid, int patternCount = -1)
    {
        m_Obstacles.Clear();

        int target = (patternCount > 0) ? patternCount : m_PatternCount;

        int attempts = 0;
        const int maxAttempts = 2000;

        if (m_Patterns == null || m_Patterns.Count == 0)
        {
            m_Patterns = new List<PatternAttributes> { line2, line3, corner, line4, L, box, snake, T }; // ADDED
        }

        while (m_Obstacles.Count < target && attempts < maxAttempts)
        {
            attempts++;

            // pick a pattern
            var pattern = m_Patterns[Random.Range(0, m_Patterns.Count)];

            // generate all transformed variants (rotations/mirrors)
            var variants = GeneratePatternVariants(pattern);

            // try the variants in random order
            Shuffle(variants);

            bool placed = false;
            foreach (var offsets in variants)
            {
                // pick random origin (avoid borders)
                Vector2Int origin = new Vector2Int(Random.Range(1, Width - 1), Random.Range(1, Height - 1));

                if (!CanPlacePattern(offsets, origin, positionsToAvoid))
                    continue;

                // Tentatively add
                var added = ApplyPatternCells(offsets, origin);

                // Verify we didn't partition the map
                if (IsBoardFullyConnected())
                {
                    placed = true;
                    break;
                }

                // rollback
                foreach (var cell in added) m_Obstacles.Remove(cell);
            }

            // If we couldn't place this attempt, loop again until attempts cap
        }

        Debug.Log($"Placed {m_Obstacles.Count} pattern cells (requested ~{target} pattern placements).");
    }

    // Generate transformed variants (permitted rotations and optional mirror)
    private List<Vector2Int[]> GeneratePatternVariants(PatternAttributes pattern)
    {
        var variants = new List<Vector2Int[]>();

        // Use degrees provided in pattern.CanRotate; ensure at least 0
        var degrees = (pattern.CanRotate != null && pattern.CanRotate.Count > 0)
            ? pattern.CanRotate.Distinct().Select(d => ((d % 360) + 360) % 360).ToList()
            : new List<int> { 0 };

        foreach (var deg in degrees)
        {
            var baseRot = RotateOffsets(pattern.Offsets, deg);
            variants.Add(baseRot);

            if (pattern.CanMirror)
            {
                variants.Add(MirrorX(baseRot));
                variants.Add(MirrorY(baseRot));
            }
        }

        // Deduplicate by signature
        var unique = new Dictionary<string, Vector2Int[]>();
        foreach (var v in variants)
        {
            var sig = Signature(NormalizeOffsets(v));
            if (!unique.ContainsKey(sig))
                unique[sig] = v;
        }

        return unique.Values.ToList();
    }

    private static Vector2Int[] RotateOffsets(List<Vector2Int> src, int deg)
    {
        Vector2Int Rot(Vector2Int o, int d)
        {
            switch (d % 360)
            {
                case 90: return new Vector2Int(-o.y, o.x);
                case 180: return new Vector2Int(-o.x, -o.y);
                case 270: return new Vector2Int(o.y, -o.x);
                default: return o;
            }
        }
        var dst = new Vector2Int[src.Count];
        for (int i = 0; i < src.Count; i++) dst[i] = Rot(src[i], deg);
        return dst;
    }

    private static Vector2Int[] RotateOffsets(Vector2Int[] src, int deg)
    {
        var list = new List<Vector2Int>(src);
        return RotateOffsets(list, deg);
    }

    private static Vector2Int[] MirrorX(Vector2Int[] src)
    {
        var dst = new Vector2Int[src.Length];
        for (int i = 0; i < src.Length; i++) dst[i] = new Vector2Int(-src[i].x, src[i].y);
        return dst;
    }

    private static Vector2Int[] MirrorY(Vector2Int[] src)
    {
        var dst = new Vector2Int[src.Length];
        for (int i = 0; i < src.Length; i++) dst[i] = new Vector2Int(src[i].x, -src[i].y);
        return dst;
    }

    private static Vector2Int[] NormalizeOffsets(Vector2Int[] src)
    {
        int minX = src.Min(v => v.x);
        int minY = src.Min(v => v.y);
        var dst = new Vector2Int[src.Length];
        for (int i = 0; i < src.Length; i++) dst[i] = new Vector2Int(src[i].x - minX, src[i].y - minY);
        return dst;
    }

    private static string Signature(Vector2Int[] src)
    {
        var copy = src.ToArray();
        System.Array.Sort(copy, (a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
        return string.Join("|", copy.Select(v => $"{v.x}_{v.y}"));
    }

    private bool CanPlace(Vector2Int coord, List<Vector2Int> avoid)
    {
        if (coord.x <= 0 || coord.y <= 0 || coord.x >= Width - 1 || coord.y >= Height - 1)
            return false;

        // overlap with existing obstacles
        if (m_Obstacles.Contains(coord)) return false;

        // reserved cells (player/treasures/etc.)
        if (avoid != null && avoid.Contains(coord)) return false;

        // CHANGED: ensure it doesn’t hit border walls implicitly
        return true;
    }

    private bool CanPlacePattern(Vector2Int[] offsets, Vector2Int origin, List<Vector2Int> avoid)
    {
        foreach (var off in offsets)
        {
            var coord = origin + off; // CHANGED: fix variable name (offChange -> off)
            if (!CanPlace(coord, avoid))
                return false;
        }
        return true;
    }

    private List<Vector2Int> ApplyPatternCells(Vector2Int[] offsets, Vector2Int origin)
    {
        var added = new List<Vector2Int>(offsets.Length);
        foreach (var off in offsets)
        {
            var p = origin + off;
            if (!m_Obstacles.Contains(p))
            {
                m_Obstacles.Add(p);
                added.Add(p);
            }
        }
        return added;
    }

    private bool IsBoardFullyConnected()
    {
        // Count passable cells and find a seed
        int totalPassable = 0;
        Vector2Int? seed = null;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                bool isBorder = (x == 0 || y == 0 || x == Width - 1 || y == Height - 1);
                bool isObstacle = m_Obstacles.Contains(new Vector2Int(x, y));
                bool passable = !isBorder && !isObstacle;
                if (passable)
                {
                    totalPassable++;
                    if (!seed.HasValue) seed = new Vector2Int(x, y);
                }
            }
        }

        if (totalPassable == 0) return true; // trivial

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
                if (m_Obstacles.Contains(n)) continue; // blocked
                visited.Add(n);
                q.Enqueue(n);
            }
        }

        return visited.Count == totalPassable;
    }

    // small util to shuffle a list
    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
