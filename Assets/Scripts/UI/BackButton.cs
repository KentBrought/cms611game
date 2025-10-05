using UnityEngine;

public class BackButton : MonoBehaviour
{
    public void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked - returning to home screen");
        GameSceneManager.Instance.LoadHomeScreen();
    }
}
