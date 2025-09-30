using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    /// <summary>
    /// This allows us to access the pause function in the PauseManager without having to add a reference to it.
    /// Not needed if we were to use the PauseManager directly,
    ///   but useful when wiring up the UI in a prefab that does not contain the PauseManager.
    /// </summary>
    public class PauseManagerProxy : MonoBehaviour
    {
        public bool isDefaultSelection = false; 
        
        public static void TogglePause()
        {
            PauseManager.TogglePauseStatic();
        }

        private void OnEnable()
        {
            if (this.isDefaultSelection)
            {
                var selectable = gameObject.GetComponent<Selectable>();
                if (selectable)
                {
                    selectable.Select();
                }
            }
        }
    }
}