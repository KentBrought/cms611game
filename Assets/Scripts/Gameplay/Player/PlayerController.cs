using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
   [SerializeField]
   private PlayerRole m_Role = PlayerRole.Robber;
   private BoardManager m_Board;
   private Vector2Int m_CellPosition;
   private bool m_IsMoving = false;
   private List<TreasureController> m_Treasures = new List<TreasureController>();

   public void Spawn(BoardManager boardManager, Vector2Int cell)
   {
       m_Board = boardManager;
       
       // Get role from RoleSelectionManager if available
       if (RoleSelectionManager.Instance != null)
       {
           m_Role = RoleSelectionManager.Instance.GetPlayerRole();
       }
       
       MoveTo(cell);
   }

   public void RegisterTreasure(TreasureController treasure)
   {
       if (!m_Treasures.Contains(treasure))
       {
           m_Treasures.Add(treasure);
       }
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
           //check if the new position is passable, then move there if it is.
           CellData cellData = m_Board.GetCellData(newCellTarget);

           if(cellData != null && cellData.Passable)
           {
               m_IsMoving = true;
               MoveTo(newCellTarget);
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
               Debug.Log("Treasure collected by robber!");
           }
       }
   }

}