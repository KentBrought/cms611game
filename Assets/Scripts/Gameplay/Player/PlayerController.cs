using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
   [SerializeField]
   private PlayerRole role = PlayerRole.Robber;

   [Header("Avatar Sprites")]
   [SerializeField] private Sprite copSprite;
   [SerializeField] private Sprite robberSprite;

   private SpriteRenderer spriteRenderer;

   private BoardManager board;
   private Vector2Int cellPosition;
   private bool isMoving = false;
   private List<TreasureController> treasures = new List<TreasureController>();
   private int collectedCoins = 0;
   
    private TurnManager turns;
    private TreasureManager treasureManager;

   private void Awake()
   {
       turns = FindFirstObjectByType<TurnManager>();
       treasureManager = FindFirstObjectByType<TreasureManager>();
       spriteRenderer = GetComponent<SpriteRenderer>();
       ApplyRoleVisual();
   }

   public void Spawn(BoardManager boardManager, Vector2Int cell)
   {
       board = boardManager;
       
        ApplyRoleVisual();
        MoveTo(cell);
   }

   public void SpawnWithRole(BoardManager boardManager, Vector2Int cell, PlayerRole newRole)
   {
       board = boardManager;
       role = newRole;
       ApplyRoleVisual();
       MoveTo(cell);
   }

   public void SetRole(PlayerRole newRole)
   {
       role = newRole;
       ApplyRoleVisual();
   }

   private void ApplyRoleVisual()
   {
       if (spriteRenderer == null) return;
       spriteRenderer.sprite = (role == PlayerRole.Cop) ? copSprite : robberSprite;
   }

#if UNITY_EDITOR
   private void OnValidate()
   {
       if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
       ApplyRoleVisual();
   }
#endif

   public void RegisterTreasure(TreasureController treasure)
   {
       if (!treasures.Contains(treasure))
       {
           treasures.Add(treasure);
       }
   }
   
   public int GetCollectedCoins()
   {
       return collectedCoins;
   }
  
   public void MoveTo(Vector2Int cell)
   {
       cellPosition = cell;
       transform.position = board.CellToWorld(cellPosition);
       
       if (role == PlayerRole.Robber)
       {
           CheckForTreasureCollection();
           CheckRobberWinCondition();
       }
       
       GameStateManager gameState = FindFirstObjectByType<GameStateManager>();
       if (gameState != null)
       {
           gameState.OnPlayerMoved(this);
       }
   }

   public PlayerRole GetRole()
   {
       return role;
   }
   
   public Vector2Int GetCellPosition()
   {
       return cellPosition;
   }

  
   private void Update()
   {
       if (isMoving) return;
       
       if (turns != null)
       {
           if (turns.GetActiveRole() != role) return;
       }

       Vector2Int newCellTarget = cellPosition;
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
           if (turns != null && !turns.CanMoveThisTurn())
           {
               return;
           }
           
           if (board == null)
           {
               Debug.LogWarning("Board is null, cannot move.");
               return;
           }
           
           CellData cellData = board.GetCellData(newCellTarget);

           if(cellData != null && cellData.Passable)
           {
               if (role == PlayerRole.Cop && treasureManager != null)
               {
                   bool isLastMove = (turns != null && turns.CanMoveThisTurn() && 
                                     turns.GetCurrentMovementSteps() + 1 >= turns.GetMaxMovementSteps());
                   
                   if (isLastMove && treasureManager.HasTreasureAt(newCellTarget))
                   {
                       return;
                   }
               }
               
               MoveDirection moveDirection = GetMoveDirection(cellPosition, newCellTarget);
               if (MoveTracker.Instance != null)
               {
                   MoveTracker.Instance.RecordMove(role, moveDirection);
               }
               
               isMoving = true;
               MoveTo(newCellTarget);
               turns?.CharacterMoved();
               Invoke(nameof(ResetMovingFlag), 0.1f);
           }
       }
   }

   private void ResetMovingFlag()
   {
       isMoving = false;
   }
   
   private MoveDirection GetMoveDirection(Vector2Int from, Vector2Int to)
   {
       Vector2Int difference = to - from;
       
       if (difference.x > 0) return MoveDirection.Right;
       if (difference.x < 0) return MoveDirection.Left;
       if (difference.y > 0) return MoveDirection.Up;
       if (difference.y < 0) return MoveDirection.Down;
       
       return MoveDirection.Right; // Default fallback
   }

   private void CheckForTreasureCollection()
   {
       foreach (TreasureController treasure in treasures)
       {
           if (treasure != null && !treasure.IsCollected() && treasure.GetCellPosition() == cellPosition)
           {
               treasure.Collect();
               collectedCoins++;
               Debug.Log("Treasure collected! Total coins: " + collectedCoins);
               
               if (turns != null)
               {
                   turns.UpdateCoinDisplay(collectedCoins);
               }
               CheckRobberWinCondition();
           }
       }
   }

   private void CheckRobberWinCondition()
   {
       if (treasureManager != null && treasureManager.AreAllTreasuresCollected())
       {
           Debug.Log("Robber wins!");
           GameSceneManager.Instance.LoadWinScreen("Robber");
       }
   }


}