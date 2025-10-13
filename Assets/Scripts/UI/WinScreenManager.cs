using UnityEngine;
using TMPro;

public class WinScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    
    private static string lastWinMessage = "Congrats! You've won.";
    
    private void Start()
    {
        if (titleText != null)
        {
            titleText.text = lastWinMessage;
        }
    }
    
    public static void SetWinMessage(string message)
    {
        lastWinMessage = message;
    }
    
    public static string GetWinMessage()
    {
        return lastWinMessage;
    }
}
