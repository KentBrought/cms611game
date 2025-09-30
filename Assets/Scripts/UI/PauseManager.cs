using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.UI
{
    public class PauseManager : MonoBehaviour//: InputHandlerBase
    {
        public static bool IsPaused { get; protected set; } = false;
        
        public GameObject pauseMenu;
        protected MyPlayerInput myPlayerInput;
        protected InputSettings.UpdateMode defaultUpdateMode;

        protected void Start()
        {
            if (!this.pauseMenu)
            {
                Debug.LogWarning($"Pause menu not assigned to PauseManager component on {this.gameObject.name}"); 
            }

            this.defaultUpdateMode = InputSystem.settings.updateMode;
            HandlePauseChange();
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
            HandlePauseChange();
        }
 
        public static void TogglePauseStatic()
        {
            var manager = FindObjectOfType<PauseManager>();

            if (!manager)
            {
                Debug.LogWarning("Could not find PauseManager object.");
                return;
            }

            manager.TogglePause();
        }

        
        public void Pause()
        {
            IsPaused = true;
            HandlePauseChange();
        }
        
        public void UnPause()
        {
            IsPaused = false;
            HandlePauseChange();
        }

        private void HandlePauseChange()
        {
            pauseMenu.SetActive(IsPaused);
            Time.timeScale = IsPaused ? 0 : 1;
            
            // If we are updating input events only in FixedUpdates, and time scale is set to zero, then no events happen.
            //   That would disable the GUI, too.
            InputSystem.settings.updateMode =
                IsPaused ? InputSettings.UpdateMode.ProcessEventsInDynamicUpdate : this.defaultUpdateMode;
        }

        protected void OnEnable()
        {
            this.myPlayerInput ??= new MyPlayerInput();
            
            this.myPlayerInput.Meta.TogglePause.Enable();
            this.myPlayerInput.Meta.TogglePause.performed += OnTogglePauseUserInput;
        }

        private void OnTogglePauseUserInput(InputAction.CallbackContext obj)
        {
            this.TogglePause();
        }

        protected void OnDisable()
        {
            this.myPlayerInput.Meta.TogglePause.Disable();
            this.myPlayerInput.Meta.TogglePause.performed -= OnTogglePauseUserInput;
        }
    }
}