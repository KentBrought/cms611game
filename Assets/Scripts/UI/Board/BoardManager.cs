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

    // list of obstacles at [(2,3), (3,3), (4,3), (7,2), (8,2), (7,5), (8,8), (9,7), (3,7), (3,8), (4,9)]
    private List<Vector2Int> m_Obstacles = new List<Vector2Int>
    {
        new Vector2Int(2, 3),
        new Vector2Int(3, 3),
        new Vector2Int(4, 3),
        new Vector2Int(7, 2),
        new Vector2Int(8, 2),
        new Vector2Int(7, 5),
        new Vector2Int(8, 8),
        new Vector2Int(9, 7),
        new Vector2Int(3, 7),
        new Vector2Int(3, 8),
        new Vector2Int(4, 8)
    };


    // Start is called before the first frame update
    public PlayerController Player;
    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

// Start is called before the first frame update
    void Start()
    {
        InitializeBoard();
    }
    
    public void InitializeBoard()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        
        m_BoardData = new CellData[Width, Height];

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
}