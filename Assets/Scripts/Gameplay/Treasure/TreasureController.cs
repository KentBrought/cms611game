using UnityEngine;

public class TreasureController : MonoBehaviour
{
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsCollected = false;

    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_Board = boardManager;
        m_CellPosition = cell;
        transform.position = m_Board.CellToWorld(m_CellPosition);
    }

    public Vector2Int GetCellPosition()
    {
        return m_CellPosition;
    }

    public bool IsCollected()
    {
        return m_IsCollected;
    }

    public void Collect()
    {
        if (!m_IsCollected)
        {
            m_IsCollected = true;
            gameObject.SetActive(false);
        }
    }
}
