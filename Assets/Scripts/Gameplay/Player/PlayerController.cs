using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
   [SerializeField]
   private PlayerRole m_Role = PlayerRole.Robber;

   [Header("Avatar Sprites")]
   [SerializeField] private Sprite copSprite;
   [SerializeField] private Sprite robberSprite;

   private SpriteRenderer m_SpriteRenderer;

   private BoardManager m_Board;
   private Vector2Int m_CellPosition;
   private bool m_IsMoving = false;
   private List<TreasureController> m_Treasures = new List<TreasureController>();
   private int m_CollectedCoins = 0;  // Track collected coins
   
    private TurnManager m_Turns;   // cache the turn manager

   private void Awake()
   {
       m_Turns = FindObjectOfType<TurnManager>();
       m_SpriteRenderer = GetComponent<SpriteRenderer>();
       ApplyRoleVisual();
   }

   public void Spawn(BoardManager boardManager, Vector2Int cell)
   {
       m_Board = boardManager;
       
       // Get role from RoleSelectionManager if available
       if (RoleSelectionManager.Instance != null)
       {
           m_Role = RoleSelectionManager.Instance.GetPlayerRole();
       }
       ApplyRoleVisual();
       MoveTo(cell);
   }
   private void ApplyRoleVisual()
   {
       if (m_SpriteRenderer == null) return;
       m_SpriteRenderer.sprite = (m_Role == PlayerRole.Cop) ? copSprite : robberSprite;
   }

#if UNITY_EDITOR
   private void OnValidate()
   {
       if (m_SpriteRenderer == null) m_SpriteRenderer = GetComponent<SpriteRenderer>();
       ApplyRoleVisual();
   }
#endif

   public void RegisterTreasure(TreasureController treasure)
   {
       if (!m_Treasures.Contains(treasure))
       {
           m_Treasures.Add(treasure);
       }
   }
   
   public int GetCollectedCoins()
   {
       return m_CollectedCoins;
   }
  
   public void MoveTo(Vector2Int cell)
   {
       m_CellPosition = cell;
       transform.position = m_Board.CellToWorld(m_CellPosition);
       
       // Check for treasure collection if player is a robber
       if (m_Role == PlayerRole.Robber)
       {
           CheckForTreasureCollection();
       }
   }

   public PlayerRole GetRole()
   {
       return m_Role;
   }

  
   private void Update()
   {
       // Prevent multiple movements in the same frame
       if (m_IsMoving) return;

       Vector2Int newCellTarget = m_CellPosition;
       bool hasMoved = false;

       if(Keyboard.current.upArrowKey.wasPressedThisFrame)
       {
           newCellTarget.y += 1;
           hasMoved = true;
       }
       else if(Keyboard.current.downArrowKey.wasPressedThisFrame)
       {
           newCellTarget.y -= 1;
           hasMoved = true;
       }
       else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
       {
           newCellTarget.x += 1;
           hasMoved = true;
       }
       else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
       {
           newCellTarget.x -= 1;
           hasMoved = true;
       }

       if(hasMoved)
       {
           // Block movement if we've already moved this turn
           if (m_Turns != null && !m_Turns.CanMoveThisTurn())
           {
               Debug.Log("Cannot move: already moved this turn.");
               return;
           }
           //check if the new position is passable, then move there if it is.
           CellData cellData = m_Board.GetCellData(newCellTarget);

           if(cellData != null && cellData.Passable)
           {
               m_IsMoving = true;
               MoveTo(newCellTarget);
               // Notify the turn system that this character just moved
               m_Turns?.CharacterMoved();
               // Reset the moving flag after a short delay
                Invoke(nameof(ResetMovingFlag), 0.1f);
           }
       }
   }

   private void ResetMovingFlag()
   {
       m_IsMoving = false;
   }

   private void CheckForTreasureCollection()
   {
       foreach (TreasureController treasure in m_Treasures)
       {
           if (treasure != null && !treasure.IsCollected() && treasure.GetCellPosition() == m_CellPosition)
           {
               treasure.Collect();
               m_CollectedCoins++;
               Debug.Log("Treasure collected by robber! Total coins: " + m_CollectedCoins);
               
               // Notify TurnManager to update UI
               if (m_Turns != null)
               {
                   m_Turns.UpdateCoinDisplay(m_CollectedCoins);
               }
           }
       }
   }

}