using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MoveReplayPad : MonoBehaviour
{
    [Header("Arrow Button References")]
    public Button upArrowButton;
    public Button downArrowButton;
    public Button leftArrowButton;
    public Button rightArrowButton;
    
    [Header("Replay Settings")]
    public float replaySpeed = 0.5f;
    public Color highlightColor = Color.yellow;
    public Color normalColor = Color.white;
    
    [Header("Pad Settings")]
    public Button replayButton;
    public Text titleText;
    
    private List<MoveDirection> currentMoves = new List<MoveDirection>();
    private PlayerRole previousPlayerRole;
    private Coroutine replayCoroutine;
    public TurnManager turnManager;
    
    private void Start()
    {

        turnManager = FindFirstObjectByType<TurnManager>();
        
        // Setup button click listeners
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(StartReplay);
        }
        
        // Initialize arrow button colors
        ResetArrowColors();
        
        // Initially hide the pad since no moves to show yet
        gameObject.SetActive(false);
    }
    
    public void ShowMovesForRole(PlayerRole role, List<MoveDirection> moves)
    {
        Debug.Log($"MoveReplayPad: ShowMovesForRole called for {role} with {moves.Count} moves");
        
        previousPlayerRole = role;
        currentMoves = new List<MoveDirection>(moves);
        
        if (moves.Count > 0)
        {
            Debug.Log("MoveReplayPad: Activating pad and starting replay");
            gameObject.SetActive(true);
            UpdateTitle();
            StartReplay();
        }
        else
        {
            Debug.Log("MoveReplayPad: No moves to show, hiding pad");
            gameObject.SetActive(false);
        }
    }
    
    public void HidePad()
    {
        if (replayCoroutine != null)
        {
            StopCoroutine(replayCoroutine);
        }
        gameObject.SetActive(false);
    }
    
    private void UpdateTitle()
    {
        if (titleText != null)
        {
            string roleName = previousPlayerRole == PlayerRole.Robber ? "Robber" : "Cop";
            titleText.text = $"{roleName}'s Last Moves";
        }
    }
    
    public void StartReplay()
    {
        if (replayCoroutine != null)
        {
            StopCoroutine(replayCoroutine);
        }
        
        replayCoroutine = StartCoroutine(ReplayMoves());
    }
    
    private IEnumerator ReplayMoves()
    {
        ResetArrowColors();
        
        if (currentMoves.Count == 0)
            yield break;
            
        yield return new WaitForSeconds(0.2f); // Small delay before starting
        
        foreach (MoveDirection move in currentMoves)
        {
            HighlightArrow(move);
            yield return new WaitForSeconds(replaySpeed);
            ResetArrowColors();
            yield return new WaitForSeconds(0.1f); // Small gap between moves
        }
        
        replayCoroutine = null;
    }
    
    private void HighlightArrow(MoveDirection direction)
    {
        ResetArrowColors(); // Reset all first
        
        Button targetButton = GetButtonForDirection(direction);
        if (targetButton != null)
        {
            ColorBlock colors = targetButton.colors;
            colors.normalColor = highlightColor;
            colors.selectedColor = highlightColor;
            targetButton.colors = colors;
        }
    }
    
    private void ResetArrowColors()
    {
        Button[] arrows = { upArrowButton, downArrowButton, leftArrowButton, rightArrowButton };
        
        foreach (Button arrow in arrows)
        {
            if (arrow != null)
            {
                ColorBlock colors = arrow.colors;
                colors.normalColor = normalColor;
                colors.selectedColor = normalColor;
                arrow.colors = colors;
            }
        }
    }
    
    private Button GetButtonForDirection(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Up:
                return upArrowButton;
            case MoveDirection.Down:
                return downArrowButton;
            case MoveDirection.Left:
                return leftArrowButton;
            case MoveDirection.Right:
                return rightArrowButton;
            default:
                return null;
        }
    }
    
    // Public method for TurnManager to call
    public void OnTurnChanged(PlayerRole newActiveRole)
    {
        Debug.Log($"MoveReplayPad: OnTurnChanged called for new active role: {newActiveRole}");
        
        if (MoveTracker.Instance != null)
        {
            // Get moves from the previous player (opposite of current active role)
            PlayerRole previousRole = (newActiveRole == PlayerRole.Robber) ? PlayerRole.Cop : PlayerRole.Robber;
            List<PlayerMove> playerMoves = MoveTracker.Instance.GetMovesForRole(previousRole);
            Debug.Log($"MoveReplayPad: Found {playerMoves.Count} moves for previous player {previousRole}");
            
            // Convert PlayerMove to MoveDirection
            List<MoveDirection> moveDirections = new List<MoveDirection>();
            foreach (PlayerMove move in playerMoves)
            {
                moveDirections.Add(move.direction);
            }
            
            ShowMovesForRole(previousRole, moveDirections);
        }
        else
        {
            Debug.LogError("MoveReplayPad: MoveTracker.Instance is null!");
        }
    }
}
