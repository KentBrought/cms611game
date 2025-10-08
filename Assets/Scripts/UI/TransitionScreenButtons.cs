using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class TransitionScreenButtons : MonoBehaviour
{
    public void OnContinueButtonClicked()
    {
        // Notify the TurnManager to continue and close the transition scene
        TurnManager turnManager = FindFirstObjectByType<TurnManager>();
        if (turnManager != null)
        {
            turnManager.ContinueAfterTransition();
        }
        else
        {
            // Fallback: directly ask the scene manager to continue
            GameSceneManager.Instance.ContinueToNextPlayer();
        }
    }

    private void Update()
    {
        // Keyboard any key
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            OnContinueButtonClicked();
            return;
        }
        // Mouse any button
        if (Mouse.current != null && (
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame))
        {
            OnContinueButtonClicked();
            return;
        }
        // Gamepad any button
        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl btn && btn.wasPressedThisFrame)
                {
                    OnContinueButtonClicked();
                    return;
                }
            }
        }
    }
}


