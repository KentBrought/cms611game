using System.Linq;
using UnityEngine;

namespace Scripts.Gameplay
{
    public class ResetToStart : MonoBehaviour
    {
        protected Vector2 startPosition;
        protected Vector2 startVelocity;
        
        protected void Start()
        {
            this.startPosition = transform.position;
            
            var rigidBody = GetComponent<Rigidbody2D>();
            if (rigidBody != null)
            {
                this.startVelocity = rigidBody.linearVelocity;
            }
        }

        public void Reset()
        {
            if (!this) return; // if this is null, then the object has been destroyed
            
            this.transform.position = this.startPosition;
            
            var rigidBody = GetComponent<Rigidbody2D>();
            if (rigidBody != null)
            {
                rigidBody.linearVelocity = this.startVelocity;
            }
        }
        
        public static void ResetAll()
        {
            var resets = FindObjectsOfType<ResetToStart>()
                .Where(reset => reset.enabled && reset.gameObject.activeInHierarchy);

            foreach (var resetMe in resets)
            {
                resetMe.Reset();
            }
        }
    }
}