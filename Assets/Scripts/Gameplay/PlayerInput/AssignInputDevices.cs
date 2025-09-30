using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Gameplay.PlayerInput
{
    public class AssignInputDevices : MonoBehaviour
    {
        void Start()
        {
            var playerInputs = this.GetComponentsInChildren<UnityEngine.InputSystem.PlayerInput>();
            var gamePads = InputSystem.devices.OfType<Gamepad>().ToArray();

            for (int i = 0; i < playerInputs.Length; ++i)
            {
                var input = playerInputs[i];
                
                if (gamePads.Length > i)
                {
                    // Map each gamepad to a PlayerInput
                    // Also bind to the keyboard so players can share it.
                    // By binding to both devices, players can use whichever one they prefer.
                    // PlayerInput also supports Auto-Switch, but I find it is not reliable with multiple players.
                    input.SwitchCurrentControlScheme(input.currentControlScheme, Keyboard.current, gamePads[i]);
                }
                else
                {
                    // No gamepad, but bind to the keyboard so players can share it.
                    input.SwitchCurrentControlScheme(input.currentControlScheme, Keyboard.current);
                }
            }
            
            
        }
    }
}