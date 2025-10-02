using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
   private BoardManager m_Board;
   private Vector2Int m_CellPosition;
   private bool m_IsMoving = false;

   public void Spawn(BoardManager boardManager, Vector2Int cell)
   {
       m_Board = boardManager;
       MoveTo(cell);
   }
  
   public void MoveTo(Vector2Int cell)
   {
       m_CellPosition = cell;
       transform.position = m_Board.CellToWorld(m_CellPosition);
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

}