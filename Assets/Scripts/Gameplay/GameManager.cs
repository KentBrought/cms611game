using Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Gameplay
{
    public class GameManager : InputHandlerBase
    {
        // Singleton pattern.
        public static GameManager Instance;

        public int score;


        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError($"GameMaster {Instance.name} already exists!  Deleting {this.name}");
                Destroy(gameObject);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            this.Restart.Enable();
            this.Restart.performed += ReloadScene;
        }

        private void ReloadScene(InputAction.CallbackContext obj)
        {
            Helpers.GeneralHelpers.ReloadScene();
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            this.Restart.Disable();
            this.Restart.performed -= ReloadScene;
        }

        public void AddScore(int scoreValue)
        {
            // Why put this in a function?
            //  Maybe we want to do something fancy with the score.
            //  Send an event to the UI to show an animation, maybe?
            this.score += scoreValue; 
        }
    }
}