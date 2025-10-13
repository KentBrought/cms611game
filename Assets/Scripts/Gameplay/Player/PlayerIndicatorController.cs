using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerIndicatorController : MonoBehaviour
{
    [Header("Indicator Settings")]
    [SerializeField] private PlayerRole indicatorForRole;
    
    private SpriteRenderer spriteRenderer;
    private BoardManager board;
    private Vector2Int cellPosition;
    private TurnManager turnManager;
    private GameStateManager gameStateManager;
    
    // Position persistence across turns for this player (but not between players)
    private Vector2Int myIndicatorPosition = Vector2Int.zero;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        turnManager = FindFirstObjectByType<TurnManager>();
        gameStateManager = FindFirstObjectByType<GameStateManager>();
        
        // Load the indicator sprite from Resources or use the one already set
        if (spriteRenderer != null && spriteRenderer.sprite == null)
        {
            // Try to load the indicator sprite
            Sprite indicatorSprite = Resources.Load<Sprite>("indicator_0");
            if (indicatorSprite != null)
            {
                spriteRenderer.sprite = indicatorSprite;
            }
        }
    }
    
    public void Initialize(BoardManager boardManager, PlayerRole role, Vector2Int startPosition)
    {
        board = boardManager;
        indicatorForRole = role;
        
        // Set initial position based on the other player's start position
        Vector2Int initialPosition = GetInitialIndicatorPosition(role, startPosition);
        MoveTo(initialPosition);
        
        // Store the position for persistence across turns
        myIndicatorPosition = initialPosition;
    }
    
    private Vector2Int GetInitialIndicatorPosition(PlayerRole role, Vector2Int otherPlayerPosition)
    {
        // Start at the exact position of the other player
        return otherPlayerPosition;
    }
    
    public void MoveTo(Vector2Int cell)
    {
        if (board == null) return;
        
        cellPosition = cell;
        transform.position = board.CellToWorld(cellPosition);
        
        // Update stored position for persistence across turns
        myIndicatorPosition = cellPosition;
        
        Debug.Log($"Indicator for {indicatorForRole} moved to position {cellPosition}");
    }
    
    private void Update()
    {
        // Only allow movement when it's this indicator's player's turn
        if (turnManager == null || turnManager.GetActiveRole() != indicatorForRole)
        {
            return;
        }
        
        // Check for WASD input
        Vector2Int newCellTarget = cellPosition;
        bool hasMoved = false;
        
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
        }
        
        if (hasMoved)
        {
            // Check if the target position is valid
            if (board != null)
            {
                CellData cellData = board.GetCellData(newCellTarget);
                if (cellData != null && cellData.Passable)
                {
                    MoveTo(newCellTarget);
                }
            }
        }
    }
    
    public Vector2Int GetCellPosition()
    {
        return cellPosition;
    }
    
    public PlayerRole GetIndicatorRole()
    {
        return indicatorForRole;
    }
    
}
