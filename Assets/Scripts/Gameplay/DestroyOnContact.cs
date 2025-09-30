using Scripts.Helpers;
using UnityEngine;

namespace Scripts.Gameplay
{
    public class DestroyOnContact : MonoBehaviour
    {
        public LayerMask thingsThatKillMe;
        public int scoreValue;
        
        // It's possible to get two OnCollisionEnter2D events in a single frame, so we will use a flag to prevent scoring twice.
        protected bool isDoomed = false;
    
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (this.isDoomed) return;
            if (!GeneralHelpers.IsInMask(this.thingsThatKillMe, other.gameObject)) return;
            
            this.isDoomed = true;
            Destroy(this.gameObject);

            if (GameManager.Instance)
            {
                GameManager.Instance.AddScore(this.scoreValue);
            }
        }
    }
}
