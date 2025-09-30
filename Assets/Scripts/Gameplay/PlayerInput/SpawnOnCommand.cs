using Scripts.Helpers;
using Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Gameplay.PlayerInput
{
    public class SpawnOnCommand : InputHandlerBase
    {
        public GameObject prefabSpawnMe;
        public Transform spawnedObjectParent;
        public SpawnInfo spawnInfo = new SpawnInfo();
        
        public void SpawnPrefabNow(InputAction.CallbackContext obj)
        {
            if (!this.enabled) return;  // Only spawn when this MonoBehaviour is enabled
            if (!obj.started) return;  // Only spawn when the button is first pressed
            if (this.prefabSpawnMe == null) return; // Only spawn if we have a prefab
            if (PauseManager.IsPaused) return; // Only fire if we are not paused
            if (!this.ShouldProcessInput) return;  // Only fire if this object is allowed to fire (usually means "not dead")
            if (IsGuiAction(obj)) return; // Only fire if the action was not a GUI interaction
            
            this.spawnInfo.Spawn(this.transform, this.prefabSpawnMe.transform, this.spawnedObjectParent);
        }
    }
}