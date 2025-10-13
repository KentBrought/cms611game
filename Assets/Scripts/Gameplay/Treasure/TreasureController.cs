using UnityEngine;

public class TreasureController : MonoBehaviour
{
    private BoardManager m_Board;
    private Vector2Int m_CellPosition;
    private bool m_IsCollected = false;
    private SpriteRenderer m_SpriteRenderer;
    private Color m_OriginalColor;

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        if (m_SpriteRenderer != null)
        {
            m_OriginalColor = m_SpriteRenderer.color;
        }
    }

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
            Debug.Log($"Treasure at {m_CellPosition} was collected by robber");
            // update visibility of treasure based on current turn
            UpdateVisibilityForCurrentTurn();
        }
    }

    public void UpdateVisibilityForCurrentTurn()
    {
        if (m_SpriteRenderer == null) return;

        if (!m_IsCollected)
        {
            // uncollected treasures are always visible in normal color
            m_SpriteRenderer.enabled = true;
            m_SpriteRenderer.color = m_OriginalColor;
        }
        else
        {
            // collected treasures visibility depends on whose turn it is
            TurnManager turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager != null)
            {
                PlayerRole activeRole = turnManager.GetActiveRole();
                UpdateVisibilityForRole(activeRole);
            }
            else
            {
                // show as greyed out if no turn manager
                m_SpriteRenderer.enabled = true;
                m_SpriteRenderer.color = Color.grey;
            }
        }
    }

    private void UpdateVisibilityForRole(PlayerRole role)
    {
        if (m_SpriteRenderer == null) return;

        if (role == PlayerRole.Robber)
        {
            // collected treasures are invisible for robber
            m_SpriteRenderer.enabled = false;
        }
        else if (role == PlayerRole.Cop)
        {
            // collected treasures are greyed out for cop
            m_SpriteRenderer.enabled = true;
            m_SpriteRenderer.color = Color.grey;
        }
    }
}
