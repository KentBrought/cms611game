using UnityEngine;

public class HomeScreenButtons : MonoBehaviour
{
    public void OnCopButtonClicked()
    {
        Debug.Log("Cop button clicked!");
        GameSceneManager.Instance.LoadMainGameAsCop();
    }
    
    public void OnRobberButtonClicked()
    {
        Debug.Log("Robber button clicked!");
        GameSceneManager.Instance.LoadMainGameAsRobber();
    }
    
    public void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked!");
        GameSceneManager.Instance.QuitGame();
    }
}
