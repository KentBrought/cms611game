using UnityEngine;

public class HomeScreenButtons : MonoBehaviour
{
    private bool m_ButtonClicked = false;
    
    public void OnCopButtonClicked()
    {
        if (m_ButtonClicked) return;
        m_ButtonClicked = true;
        
        Debug.Log("Cop button clicked!");
        GameSceneManager.Instance.LoadMainGameAsCop();
    }
    
    public void OnRobberButtonClicked()
    {
        if (m_ButtonClicked) return;
        m_ButtonClicked = true;
        
        Debug.Log("Robber button clicked!");
        GameSceneManager.Instance.LoadMainGameAsRobber();
    }
    
    public void OnQuitButtonClicked()
    {
        if (m_ButtonClicked) return;
        m_ButtonClicked = true;
        
        Debug.Log("Quit button clicked!");
        GameSceneManager.Instance.QuitGame();
    }
}
