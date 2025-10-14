using UnityEngine;
using System.Collections.Generic;

public class SirenManager : MonoBehaviour
{
    [SerializeField] private AudioSource sirenAudioSource;
    [SerializeField] private AudioClip sirenClip;
    private BoardManager board;
    private bool isSirenActive = false;
    private PlayerController cop;
    private PlayerController robber;
    private TurnManager turnManager;

    private void Start()
    {
        if (sirenAudioSource == null)
        {
            sirenAudioSource = gameObject.AddComponent<AudioSource>();
        }
        sirenAudioSource.clip = sirenClip;
        sirenAudioSource.loop = true;
        sirenAudioSource.playOnAwake = false;
        

        board = FindFirstObjectByType<BoardManager>();
        turnManager = FindFirstObjectByType<TurnManager>();
        
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player.GetRole() == PlayerRole.Cop)
                cop = player;
            else if (player.GetRole() == PlayerRole.Robber)
                robber = player;
        }
    }

    public void PlaySiren()
    {
        if (!sirenAudioSource.isPlaying)
        {
            sirenAudioSource.Play();
        }
    }

    public void StopSiren()
    {
        if (sirenAudioSource.isPlaying)
        {
            sirenAudioSource.Stop();
        }
        isSirenActive = false;
    }
    
    private void Update()
    {
        // Only check for siren activation during robber's turn
        if (turnManager != null && turnManager.GetActiveRole() == PlayerRole.Robber)
        {
            CheckSirenActivation();
        }
        else if (isSirenActive)
        {
            // Stop siren if it's not robber's turn
            StopSiren();
        }
    }
    
    private void CheckSirenActivation()
    {
        if (cop == null || robber == null || board == null)
            return;
            
        Vector2Int copPos = cop.GetCellPosition();
        Vector2Int robberPos = robber.GetCellPosition();
        
        bool hasLineOfSight = HasClearLineOfSight(copPos, robberPos);
        
        if (hasLineOfSight && !isSirenActive)
        {
            isSirenActive = true;
            PlaySiren();
        }
        else if (!hasLineOfSight && isSirenActive)
        {
            isSirenActive = false;
            StopSiren();
        }
    }
    
    private bool HasClearLineOfSight(Vector2Int copPos, Vector2Int robberPos)
    {
        // Check if cop and robber are in the same row or column
        bool sameRow = copPos.y == robberPos.y;
        bool sameColumn = copPos.x == robberPos.x;
        
        if (!sameRow && !sameColumn)
            return false;
            
        // Check for obstacles between cop and robber
        if (sameRow)
        {
            return CheckHorizontalLineOfSight(copPos, robberPos);
        }
        else // sameColumn
        {
            return CheckVerticalLineOfSight(copPos, robberPos);
        }
    }
    
    private bool CheckHorizontalLineOfSight(Vector2Int copPos, Vector2Int robberPos)
    {
        int startX = Mathf.Min(copPos.x, robberPos.x) + 1;
        int endX = Mathf.Max(copPos.x, robberPos.x) - 1;
        int y = copPos.y;
        
        // Check each cell between cop and robber
        for (int x = startX; x <= endX; x++)
        {
            CellData cellData = board.GetCellData(new Vector2Int(x, y));
            if (cellData == null || !cellData.Passable)
            {
                return false; // Obstacle found
            }
        }
        
        return true; // Clear line of sight
    }
    
    private bool CheckVerticalLineOfSight(Vector2Int copPos, Vector2Int robberPos)
    {
        int startY = Mathf.Min(copPos.y, robberPos.y) + 1;
        int endY = Mathf.Max(copPos.y, robberPos.y) - 1;
        int x = copPos.x;
        
        // Check each cell between cop and robber
        for (int y = startY; y <= endY; y++)
        {
            CellData cellData = board.GetCellData(new Vector2Int(x, y));
            if (cellData == null || !cellData.Passable)
            {
                return false; // Obstacle found
            }
        }
        
        return true; // Clear line of sight
    }
    
    public bool IsSirenActive()
    {
        return isSirenActive;
    }
}