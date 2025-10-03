using UnityEngine;
using System.Collections.Generic;

public class TreasureManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_TreasurePrefab;
    
    [SerializeField]
    private int m_TreasureCount = 5;
    
    private BoardManager m_Board;
    private List<TreasureController> m_Treasures = new List<TreasureController>();
    private List<PlayerController> m_Players = new List<PlayerController>();

    public void Initialize(BoardManager board, int treasureCount = 5)
    {
        m_Board = board;
        m_TreasureCount = treasureCount;
        
        // Hide the template treasure prefab
        HideTreasureTemplate();
        
        SpawnTreasures();
    }

    public void RegisterPlayer(PlayerController player)
    {
        if (!m_Players.Contains(player))
        {
            m_Players.Add(player);
            
            // Register all existing treasures with the new player
            foreach (TreasureController treasure in m_Treasures)
            {
                player.RegisterTreasure(treasure);
            }
        }
    }

    private void SpawnTreasures()
    {
        for (int i = 0; i < m_TreasureCount; i++)
        {
            Vector2Int treasurePosition = GetRandomPassablePosition();
            SpawnTreasureAt(treasurePosition);
        }
    }

    private Vector2Int GetRandomPassablePosition()
    {
        Vector2Int position;
        int attempts = 0;
        const int maxAttempts = 100;

        do
        {
            position = new Vector2Int(
                Random.Range(1, m_Board.Width - 1),
                Random.Range(1, m_Board.Height - 1)
            );
            attempts++;
        }
        while (attempts < maxAttempts && 
               (!IsPositionPassable(position) || IsPositionOccupied(position)));

        return position;
    }

    private bool IsPositionPassable(Vector2Int position)
    {
        CellData cellData = m_Board.GetCellData(position);
        return cellData != null && cellData.Passable;
    }

    private bool IsPositionOccupied(Vector2Int position)
    {
        foreach (TreasureController treasure in m_Treasures)
        {
            if (treasure.GetCellPosition() == position)
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnTreasureAt(Vector2Int position)
    {
        if (m_TreasurePrefab != null)
        {
            GameObject treasureObject = Instantiate(m_TreasurePrefab);
            treasureObject.SetActive(true); // Ensure the clone is active
            TreasureController treasure = treasureObject.GetComponent<TreasureController>();
            
            if (treasure != null)
            {
                treasure.Spawn(m_Board, position);
                m_Treasures.Add(treasure);
                
                // Register with all existing players
                foreach (PlayerController player in m_Players)
                {
                    player.RegisterTreasure(treasure);
                }
            }
        }
    }
    
    public bool HasTreasureAt(Vector2Int position)
    {
        foreach (TreasureController treasure in m_Treasures)
        {
            if (treasure != null && treasure.GetCellPosition() == position)
            {
                return true;
            }
        }
        return false;
    }
    
    private void HideTreasureTemplate()
    {
        if (m_TreasurePrefab != null)
        {
            m_TreasurePrefab.SetActive(false);
        }
    }
}
